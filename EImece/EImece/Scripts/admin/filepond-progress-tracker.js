/**
 * FilePondOverallProgressTracker
 * Real-time overall upload progress tracking and metrics calculation with EWMA speed smoothing.
 */
(function (root, factory) {
    if (typeof define === 'function' && define.amd) {
        define(['jquery'], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory(require('jquery'));
    } else {
        root.FilePondOverallProgressTracker = factory(root.jQuery || root.$);
    }
}(typeof window !== 'undefined' ? window : this, function ($) {
    'use strict';

    function FilePondOverallProgressTracker(options) {
        this.options = $.extend({
            container: '.filepond-overall-progress-card',
            lang: {
                uploadingTitle: 'Fotoğraflar yükleniyor',
                completingTitle: 'Sunucu işlemi tamamlanıyor...',
                completedTitle: 'Tüm fotoğraflar başarıyla yüklendi',
                errorTitle: 'Bazı fotoğraflar yüklenemedi',
                cancelledTitle: 'Yükleme iptal edildi',
                imagesUploaded: '{done} / {total} fotoğraf yüklendi',
                allImagesUploaded: 'Tüm {total} fotoğraf yüklendi',
                calculating: 'Hesaplanıyor...',
                secLeft: '~{sec} sn kaldı',
                minSecLeft: '~{min} dk {sec} sn kaldı',
                hoursLeft: '~{hours} sa {min} dk kaldı',
                justNow: 'Tamamlanıyor...'
            }
        }, options);

        this.$container = $(this.options.container);
        this.$statusTitle = this.$container.find('.fpop-status-title');
        this.$percentage = this.$container.find('.fpop-percentage');
        this.$fileCount = this.$container.find('.fpop-file-count-text');
        this.$barFill = this.$container.find('.fpop-bar-fill');
        this.$speedText = this.$container.find('.fpop-speed-text');
        this.$bytesText = this.$container.find('.fpop-bytes-text');
        this.$etaText = this.$container.find('.fpop-eta-text');

        this.files = {}; // fileId -> { size: Number, progress: Number, status: Number, name: String }
        this.state = 'idle'; // 'idle' | 'uploading' | 'completing' | 'completed' | 'error' | 'cancelled'
        
        // Speed & ETA tracking with Exponentially Weighted Moving Average (EWMA)
        this.uploadStartTime = null;
        this.lastSampleTime = null;
        this.lastSampleBytes = 0;
        this.smoothedBytesPerSec = 0;
        this.alpha = 0.25; // EWMA smoothing factor
        this.rafId = null;
        this.isDirty = false;
    }

    FilePondOverallProgressTracker.prototype.formatBytes = function (bytes) {
        if (!bytes || bytes <= 0) return '0 B';
        var k = 1024;
        var sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
        var i = Math.floor(Math.log(bytes) / Math.log(k));
        if (i < 0) i = 0;
        if (i >= sizes.length) i = sizes.length - 1;
        return parseFloat((bytes / Math.pow(k, i)).toFixed(i === 0 ? 0 : (i === 1 ? 1 : 2))) + ' ' + sizes[i];
    };

    FilePondOverallProgressTracker.prototype.formatSpeed = function (bytesPerSec) {
        if (!bytesPerSec || bytesPerSec <= 0) return '-- Mbit/s';
        var bitsPerSec = bytesPerSec * 8;
        if (bitsPerSec >= 1000 * 1000 * 1000) {
            return (bitsPerSec / (1000 * 1000 * 1000)).toFixed(2) + ' Gbit/s';
        }
        if (bitsPerSec >= 1000 * 1000) {
            return (bitsPerSec / (1000 * 1000)).toFixed(2) + ' Mbit/s';
        }
        if (bitsPerSec >= 1000) {
            return (bitsPerSec / 1000).toFixed(1) + ' Kbit/s';
        }
        return Math.round(bitsPerSec) + ' bit/s';
    };

    FilePondOverallProgressTracker.prototype.formatETA = function (seconds) {
        var lang = this.options.lang;
        if (seconds === null || isNaN(seconds) || seconds === undefined || !isFinite(seconds)) {
            return lang.calculating;
        }
        var s = Math.round(seconds);
        if (s <= 1) {
            return lang.justNow;
        }
        if (s < 60) {
            return lang.secLeft.replace('{sec}', s);
        }
        var mins = Math.floor(s / 60);
        var remSecs = s % 60;
        if (mins < 60) {
            return lang.minSecLeft.replace('{min}', mins).replace('{sec}', remSecs < 10 ? '0' + remSecs : remSecs);
        }
        var hours = Math.floor(mins / 60);
        var remMins = mins % 60;
        return lang.hoursLeft.replace('{hours}', hours).replace('{min}', remMins);
    };

    FilePondOverallProgressTracker.prototype.addFile = function (fileItem) {
        if (!fileItem || !fileItem.id) return;
        var size = fileItem.fileSize || (fileItem.file ? fileItem.file.size : 0);
        this.files[fileItem.id] = {
            size: size,
            progress: fileItem.status === 5 /* COMPLETE */ ? 1.0 : 0,
            status: fileItem.status,
            name: fileItem.filename || (fileItem.file ? fileItem.file.name : '')
        };
        this.scheduleRender();
    };

    FilePondOverallProgressTracker.prototype.updateProgress = function (fileItem, progress) {
        if (!fileItem || !fileItem.id) return;
        var entry = this.files[fileItem.id];
        if (!entry) {
            this.addFile(fileItem);
            entry = this.files[fileItem.id];
        }
        var clampedProgress = Math.max(0, Math.min(1, progress || 0));
        entry.progress = clampedProgress;
        entry.status = fileItem.status;
        
        this.updateSpeedAndETA();
        this.scheduleRender();
    };

    FilePondOverallProgressTracker.prototype.setFileComplete = function (fileItem, error) {
        if (!fileItem || !fileItem.id) return;
        var entry = this.files[fileItem.id];
        if (!entry) {
            this.addFile(fileItem);
            entry = this.files[fileItem.id];
        }
        if (error) {
            entry.status = 8; /* LOAD_ERROR / ERROR */
        } else {
            entry.status = 5; /* COMPLETE */
            entry.progress = 1.0;
        }
        this.updateSpeedAndETA();
        this.scheduleRender();
    };

    FilePondOverallProgressTracker.prototype.removeFile = function (fileId) {
        if (this.files[fileId]) {
            delete this.files[fileId];
            this.updateSpeedAndETA();
            this.scheduleRender();
        }
    };

    FilePondOverallProgressTracker.prototype.startProcessing = function () {
        this.state = 'uploading';
        this.uploadStartTime = Date.now();
        this.lastSampleTime = this.uploadStartTime;
        this.lastSampleBytes = this.getUploadedBytes();
        this.smoothedBytesPerSec = 0;
        this.$container.slideDown(200);
        this.scheduleRender();
    };

    FilePondOverallProgressTracker.prototype.getUploadedBytes = function () {
        var total = 0;
        for (var id in this.files) {
            if (this.files.hasOwnProperty(id)) {
                var f = this.files[id];
                total += (f.size * f.progress);
            }
        }
        return total;
    };

    FilePondOverallProgressTracker.prototype.getTotalBytes = function () {
        var total = 0;
        for (var id in this.files) {
            if (this.files.hasOwnProperty(id)) {
                total += this.files[id].size;
            }
        }
        return total;
    };

    FilePondOverallProgressTracker.prototype.getCounts = function () {
        var total = 0;
        var completed = 0;
        var errors = 0;
        var processing = 0;
        for (var id in this.files) {
            if (this.files.hasOwnProperty(id)) {
                total++;
                var st = this.files[id].status;
                if (st === 5 /* COMPLETE */) {
                    completed++;
                } else if (st === 8 || st === 7 /* ERROR */) {
                    errors++;
                } else if (st === 3 || st === 4 /* PROCESSING */) {
                    processing++;
                }
            }
        }
        return { total: total, completed: completed, errors: errors, processing: processing };
    };

    FilePondOverallProgressTracker.prototype.updateSpeedAndETA = function () {
        var now = Date.now();
        if (!this.lastSampleTime) {
            this.lastSampleTime = now;
            this.lastSampleBytes = this.getUploadedBytes();
            return;
        }

        var timeDelta = (now - this.lastSampleTime) / 1000;
        if (timeDelta >= 0.15) { // Sample every 150ms minimum
            var currentBytes = this.getUploadedBytes();
            var bytesDelta = Math.max(0, currentBytes - this.lastSampleBytes);
            var instantSpeed = bytesDelta / timeDelta;

            if (this.smoothedBytesPerSec === 0) {
                this.smoothedBytesPerSec = instantSpeed;
            } else {
                // EWMA smoothing filter
                this.smoothedBytesPerSec = (this.alpha * instantSpeed) + ((1 - this.alpha) * this.smoothedBytesPerSec);
            }

            this.lastSampleTime = now;
            this.lastSampleBytes = currentBytes;
        }
    };

    FilePondOverallProgressTracker.prototype.scheduleRender = function () {
        if (!this.isDirty) {
            this.isDirty = true;
            var self = this;
            if (window.requestAnimationFrame) {
                this.rafId = window.requestAnimationFrame(function () {
                    self.render();
                });
            } else {
                setTimeout(function () {
                    self.render();
                }, 16);
            }
        }
    };

    FilePondOverallProgressTracker.prototype.render = function () {
        this.isDirty = false;
        var counts = this.getCounts();
        var totalBytes = this.getTotalBytes();
        var uploadedBytes = this.getUploadedBytes();
        var percent = totalBytes > 0 ? (uploadedBytes / totalBytes) * 100 : 0;
        var lang = this.options.lang;

        if (counts.total === 0) {
            this.state = 'idle';
            this.$container.slideUp(200);
            return;
        }

        // Determine state
        if (counts.errors > 0 && counts.processing === 0 && counts.completed < counts.total) {
            this.state = 'error';
        } else if (counts.completed === counts.total && counts.total > 0) {
            this.state = 'completed';
        } else if (percent >= 99.5 && counts.completed < counts.total) {
            this.state = 'completing';
        } else if (counts.processing > 0 || (this.uploadStartTime && counts.completed < counts.total)) {
            this.state = 'uploading';
        }

        // Apply state classes
        this.$container
            .removeClass('state-idle state-uploading state-completing state-completed state-error')
            .addClass('state-' + this.state);

        // Update progress bar
        var clampedPercent = Math.min(100, Math.max(0, percent));
        this.$barFill.css('width', clampedPercent.toFixed(1) + '%');
        this.$barFill.attr('aria-valuenow', Math.round(clampedPercent));

        // Update percentage readout
        this.$percentage.text(Math.round(clampedPercent) + '%');

        // Update file count row
        if (this.state === 'completed') {
            this.$statusTitle.text('✓ ' + lang.completedTitle);
            this.$fileCount.text(lang.allImagesUploaded.replace('{total}', counts.total));
            this.$speedText.text(this.formatBytes(totalBytes) + ' yüklendi');
            this.$bytesText.text(this.formatBytes(totalBytes) + ' / ' + this.formatBytes(totalBytes));
            this.$etaText.text('Tamamlandı ✓');
        } else if (this.state === 'error') {
            this.$statusTitle.text('⚠ ' + lang.errorTitle);
            this.$fileCount.text(lang.imagesUploaded.replace('{done}', counts.completed).replace('{total}', counts.total));
            this.$speedText.text(this.formatSpeed(this.smoothedBytesPerSec));
            this.$bytesText.text(this.formatBytes(uploadedBytes) + ' / ' + this.formatBytes(totalBytes));
            this.$etaText.text(counts.errors + ' hata');
        } else if (this.state === 'completing') {
            this.$statusTitle.text(lang.completingTitle);
            this.$fileCount.text(lang.imagesUploaded.replace('{done}', counts.completed).replace('{total}', counts.total));
            this.$speedText.text(this.formatSpeed(this.smoothedBytesPerSec));
            this.$bytesText.text(this.formatBytes(uploadedBytes) + ' / ' + this.formatBytes(totalBytes));
            this.$etaText.text(lang.justNow);
        } else {
            // Uploading
            this.$statusTitle.text(lang.uploadingTitle);
            this.$fileCount.text(lang.imagesUploaded.replace('{done}', counts.completed).replace('{total}', counts.total));
            this.$speedText.text(this.formatSpeed(this.smoothedBytesPerSec));
            this.$bytesText.text(this.formatBytes(uploadedBytes) + ' / ' + this.formatBytes(totalBytes));

            var remainingBytes = Math.max(0, totalBytes - uploadedBytes);
            var eta = (this.smoothedBytesPerSec > 1024 && remainingBytes > 0)
                ? (remainingBytes / this.smoothedBytesPerSec)
                : null;

            this.$etaText.text(this.formatETA(eta));
        }

        if (this.state !== 'idle') {
            this.$container.show();
        }
    };

    FilePondOverallProgressTracker.prototype.reset = function () {
        this.files = {};
        this.state = 'idle';
        this.uploadStartTime = null;
        this.lastSampleTime = null;
        this.lastSampleBytes = 0;
        this.smoothedBytesPerSec = 0;
        this.$barFill.css('width', '0%').attr('aria-valuenow', 0);
        this.$percentage.text('0%');
        this.$container.slideUp(150);
    };

    return FilePondOverallProgressTracker;
}));

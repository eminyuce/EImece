/**
 * EImece Admin data grid — Grid.Mvc / MVCGrid replacement helpers.
 * Requires jQuery. Works with Bootstrap 5 markup from EntityList.
 */
(function ($) {
  'use strict';

  function getAntiForgeryToken() {
    return $('input[name="__RequestVerificationToken"]').first().val()
      || $('meta[name="RequestVerificationToken"]').attr('content');
  }

  $(function () {
    var $grid = $('[data-admin-grid]');
    if (!$grid.length) return;

    // Row checkbox highlight (legacy gridChecked parity)
    $grid.on('change', 'input[name=checkboxGrid]', function () {
      $(this).closest('tr').toggleClass('gridChecked', this.checked);
    });

    // Select all
    $grid.on('change', '[data-grid-select-all]', function () {
      var on = this.checked;
      $grid.find('input[name=checkboxGrid]').prop('checked', on).trigger('change');
    });

    // Bulk soft-delete via Admin/Ajax
    $grid.on('click', '[data-grid-delete-selected]', function (e) {
      e.preventDefault();
      var action = $(this).data('grid-delete-selected');
      if (!action) return;
      var ids = $grid.find('input[name=checkboxGrid]:checked').map(function () {
        return $(this).val();
      }).get();
      if (!ids.length) {
        alert('Lütfen silinecek satırları seçin.');
        return;
      }
      if (!confirm(ids.length + ' kayıt pasifleştirilsin mi?')) return;

      $.ajax({
        url: '/Admin/Ajax/' + action,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ values: ids.join(',') }),
        headers: { 'RequestVerificationToken': getAntiForgeryToken() }
      }).done(function () {
        window.location.reload();
      }).fail(function (xhr) {
        alert('Silme başarısız: ' + (xhr.responseText || xhr.status));
      });
      // Note: Admin/Ajax delete endpoints return a JSON array of ids (legacy adminEimece shape).
    });

    // Page size change
    $grid.on('change', '[data-grid-page-size]', function () {
      var size = $(this).val();
      var url = new URL(window.location.href);
      url.searchParams.set('pageSize', size);
      url.searchParams.set('page', '1');
      window.location = url.toString();
    });

    // Client filter box (filters current page quickly)
    $grid.on('input', '[data-grid-client-filter]', function () {
      var q = ($(this).val() || '').toString().toLowerCase();
      $grid.find('tbody tr[data-grid-row]').each(function () {
        var text = $(this).text().toLowerCase();
        $(this).toggle(!q || text.indexOf(q) >= 0);
      });
    });
  });
})(jQuery);

/*
*
*  Push Notifications codelab
*  Copyright 2015 Google Inc. All rights reserved.
*
*  Licensed under the Apache License, Version 2.0 (the "License");
*  you may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      https://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License
*
*/
/* eslint-env browser, serviceworker, es6 */

'use strict';


var redirectionUrl = '';
var notificationSubscriberId = 0;
const NOTIFICATION_DELAY = 2500;
const PushNotificationApplicationServerUrl = '';



const promiseTimeout = function (cb, timeout) {
    return new Promise((resolve) => {
        setTimeout(() => {
            cb();
            resolve();
        }, timeout);
    });
};

self.addEventListener('push', function (event) {
    console.log('[Service Worker] Push Received.');


    event.waitUntil(function () {


        var jsonDataObj = JSON.parse(event.data.text());
        console.log('[Service Worker] Push Received.' + event.data.text());

        var title = jsonDataObj.title;
        var body = jsonDataObj.body;
        var imageUrl = jsonDataObj.imageurl;
        var notificationType = jsonDataObj.notificationtype;
        notificationSubscriberId = jsonDataObj.notificationSubscriberId;
        redirectionUrl = jsonDataObj.redirectionurl;





        if (notificationType == "Debugging") {

            console.log('Service Worker Push Received For Debugging');
            console.log('Title:' + title);
            console.log('Body:' + body);
            console.log('ImageUrl:' + imageUrl);
            sendFeedBack(notificationSubscriberId, 3);

            return new Promise((resolve) => { });

        } else if (notificationType == "SimpleNotification") {
            console.log('[Service Worker] Push Received.1');

            sendFeedBack(notificationSubscriberId, 1);

            var options = {
                body: body,
                icon: imageUrl
            };
            return self.registration.showNotification(title, options);



        } else if (notificationType == "UpdateServiceWorker") {
            //https://developer.mozilla.org/en-US/docs/Web/API/ServiceWorkerRegistration/update
            console.log('Service Worker has been updated.');

            sendFeedBack(notificationSubscriberId, 8);

            return self.registration.update();
        } else if (notificationType == "UnRegisterServiceWorker") {
            //https://developer.mozilla.org/en-US/docs/Web/API/ServiceWorkerRegistration/unregister
            console.log('Service Worker has been unregister notification.');

            self.registration.unregister().then(function (boolean) {
                // if boolean = true, unregister is successful
                console.log('Registration result:' + boolean);
                if (boolean) {

                    sendFeedBack(notificationSubscriberId, 2);
                } else {

                }
            });


            return new Promise((resolve) => { });
        }



    }());

});
function sendFeedBack(notificationSubscriberId, notificationStatus) {
    var postData = JSON.stringify({
        "notificationSubscriberId": notificationSubscriberId,
        "notificationStatus": notificationStatus
    });

    console.log(postData);
    ajaxMethodCall(postData, PushNotificationApplicationServerUrl + "/Ajax/GetSubscribersTracking", function (data) {
        console.log(data);
    });

}
self.addEventListener('notificationclick', function (event) {
    console.log('[Service Worker] Notification click Received.');
    console.log(event);
    event.notification.close();
    sendFeedBack(notificationSubscriberId, 9);

    event.waitUntil(

        clients.openWindow(redirectionUrl)
   );
});
//The install event is the first event a service worker gets, and it only happens once.
self.addEventListener('install', event => {
    console.log('V1 installing…');
});

self.addEventListener('activate', event => {
    console.log('V1 now ready to handle fetches!');
});

function ajaxMethodCall(data, url, success) {

    let headers = new Headers({
        // 'Access-Control-Allow-Origin': '*',
        'Content-Type': 'application/json; charset=utf-8'
    });

    return fetch(url, {
        method: 'post',
        mode: 'cors',
        headers: headers,
        body: data
    })
    .then(function (response) {
        return response.json();
    })
    .then(function (result) {
        console.log(result);
    })
    .catch(function (error) {
        console.log('Request failed', error);
    });



}

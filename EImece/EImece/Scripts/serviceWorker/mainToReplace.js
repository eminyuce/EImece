'use strict';

const applicationServerPublicKeyChrome = '[ChromePublicKey]';
const applicationServerPublicKeyFirefox = '[FirefoxPublickKey]';
const applicationServerPublicKeySafari = '[SafariPublicKey]';

const PushNotificationApplicationServerUrl = '[ApplicationServerUrl]';

let isSubscribed = false;
let swRegistration = null;

function urlB64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
      .replace(/\-/g, '+')
      .replace(/_/g, '/');

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}
function GetApplicationServerPublicKey() {
    var applicationServerPublicKey = applicationServerPublicKeyChrome;
    if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
        applicationServerPublicKey = applicationServerPublicKeyChrome;
    } else if (navigator.userAgent.toLowerCase().indexOf('firefox') > -1) {
        applicationServerPublicKey = applicationServerPublicKeyFirefox;
    } else if (navigator.userAgent.toLowerCase().indexOf('safari') > -1) {
        applicationServerPublicKey = applicationServerPublicKeySafari;
    }

    return applicationServerPublicKey;
}
function sendSubriberInfoToServer(subscription) {

    var applicationServerPublicKey = GetApplicationServerPublicKey();


    var jsonDataSubsriber = JSON.stringify(subscription);
    var jsonDataSubsriberObj = JSON.parse(jsonDataSubsriber);
    var postData = JSON.stringify({
        "endpoint": jsonDataSubsriberObj.endpoint,
        "p256dh": jsonDataSubsriberObj.keys.p256dh,
        "auth": jsonDataSubsriberObj.keys.auth,
        "applicationServerPublicKey": applicationServerPublicKey,
        "userAgent": navigator.userAgent
    });
    ajaxMethodCall(postData, PushNotificationApplicationServerUrl + "/Ajax/GetSubscribersData", function (data) {
        console.log(data);
    });

}
function initializeUI() {


    // Set the initial subscription value
    swRegistration.pushManager.getSubscription()
    .then(function (subscription) {
        isSubscribed = !(subscription === null);

        if (!isSubscribed) {
            subscribeUser();
        }  

    });

  //  swRegistration.pushManager.getSubscription()
  //.then(function (subscription) {
  //    if (subscription) {
  //        // TODO: Tell application server to delete subscription
  //        return subscription.unsubscribe();
  //    }
  //})
  //.catch(function (error) {
  //    console.log('Error unsubscribing', error);
  //}).then(function () {
  //    console.log('User is unsubscribed.');
  //    isSubscribed = false;
  //})

}

//function unsubscribeUser() {
//    swRegistration.pushManager.getSubscription()
//    .then(function (subscription) {
//        if (subscription) {
//            return subscription.unsubscribe();
//        }
//    })
//    .catch(function (error) {
//        console.log('Error unsubscribing', error);
//    })
//    .then(function () {
//        console.log('User is unsubscribed.');
//        isSubscribed = false;
//    });
//}
 
function subscribeUser() {

    var applicationServerPublicKey = GetApplicationServerPublicKey();

    const applicationServerKey = urlB64ToUint8Array(applicationServerPublicKey);
    swRegistration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: applicationServerKey
    })
    .then(function (subscription) {
        console.log('User is subscribed 1.');
        isSubscribed = true;
        sendSubriberInfoToServer(subscription);
    })
    .catch(function (err) {
        console.log('Failed to subscribe the user: ', err);

    });
}


if ('serviceWorker' in navigator && 'PushManager' in window) {
    console.log('Service Worker and Push is supported');

    navigator.serviceWorker.register('/localSw.js?v=[LocalSwVersion]')
  .then(function (swReg) {
      console.log('Service Worker is registered', swReg);



      swRegistration = swReg;
      initializeUI();
  })
  .catch(function (error) {
      console.error('Service Worker Error', error);
  });
} else {
    console.warn('Push messaging is not supported');

}

function ajaxMethodCall(postData, ajaxUrl, successFunction) {

    $.ajax({
        type: "POST",
        url: ajaxUrl,
        data: postData,
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: successFunction,
        error: function (jqXHR, exception) {
            console.error("parameters :" + postData);
            console.error("ajaxUrl :" + ajaxUrl);
            console.error("responseText :" + jqXHR.responseText);
            if (jqXHR.status === 0) {
                console.error('Not connect.\n Verify Network.');
            } else if (jqXHR.status == 404) {
                console.error('Requested page not found. [404]');
            } else if (jqXHR.status == 500) {
                console.error('Internal Server Error [500].');
            } else if (exception === 'parsererror') {
                console.error('Requested JSON parse failed.');
            } else if (exception === 'timeout') {
                console.error('Time out error.');
            } else if (exception === 'abort') {
                console.error('Ajax request aborted.');
            } else {
                console.error('Uncaught Error.\n' + jqXHR.responseText);
            }
        }
    });
}
(function () {
    var div = document.createElement('div');
    var cookieEnum = {
        accept: '9',
        scroll: '5',
        close: '3'
    };
    var cookieName = 'GDPR';

    window.CookieConsent = function (message, privacyLink, termsLink) {
        var currentCookie = getCookie(cookieName);
        if (currentCookie) {
            console.log(currentCookie + " <-- Selected Cookie");
            return;
        }
        var privacyLink = privacyLink || 'https://www.google.com';
        var termsLink = termsLink || 'https://www.google.com';
        message = message || "<div>We use cookies to improve your website experience. By continuing to use our site, you accept our use of cookies, revised  <a style='color:white;' href='" + privacyLink + "'>Privacy Policy</a> and <a  style='color:white;' href='" + termsLink + "'>Terms of Use.</a></div><div><button>Accept</button></div> <div><button>Close</button></div>";
        div.innerHTML = message;
        div = addDivStyles(div);
        var acceptButton = div.getElementsByTagName('button')[0];
        var closeButton = div.getElementsByTagName('button')[1];

        acceptButton = addButtonStyle(acceptButton);
        closeButton = addButtonStyle(closeButton);
        closeButton = addCloseStyle(closeButton);

        window.addEventListener('scroll', CookieScrolled);
        acceptButton.addEventListener('click', CookieAccepted);
        closeButton.addEventListener('click', CookieClosed);

        document.getElementsByTagName('body')[0].appendChild(div);
    };

    function CookieScrolled() {
        setCookie(cookieName, cookieEnum.scroll);
        console.log('scrolled');
        window.removeEventListener('scroll', CookieScrolled);
        div.remove();
    }
    function CookieAccepted() {
        setCookie(cookieName, cookieEnum.accept);
        console.log('accept clicked');
        div.remove();
    }

    function CookieClosed() {
        setCookie(cookieName, cookieEnum.close);
        console.log('close clicked');
        div.remove();
    }

    function addDivStyles(el) {
        el.style.display = 'flex';
        el.style.position = 'fixed';
        el.style.bottom = '0';
        el.style.right = '0';
        el.style.left = '0';
        el.style.padding = '20px';
        el.style.background = '#353535';
        el.style.color = '#ffffff';
        el.style.fontFamily = 'Verdana, Geneva, sans-serif';
        el.style.border = '1px solid #000000';
        return el;
    }

    function addButtonStyle(el) {
        el.style.padding = '10px';
        el.style.margin = '10px';
        el.style.border = '0';
        el.style.cursor = 'pointer';
        el.style.background = '#00901E';
        el.style.color = '#ffffff';
        return el;
    }
    function addCloseStyle(el) {
        el.style.background = 'url("data:image/svg+xml;base64,PHN2ZyB2ZXJzaW9uPSIxLjEiIGlkPSJMYXllcl8xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHhtbG5zOnhsaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hsaW5rIiB4PSIwcHgiIHk9IjBweCINCgkgdmlld0JveD0iMCAwIDc0IDc0IiBzdHlsZT0iZW5hYmxlLWJhY2tncm91bmQ6bmV3IDAgMCA3NCA3NDsiIHhtbDpzcGFjZT0icHJlc2VydmUiPg0KPHN0eWxlIHR5cGU9InRleHQvY3NzIj4NCgkuc3Qwe2ZpbGw6I0NDQ0NDQzt9DQoJLnN0MXtmaWxsOiNGRkZGRkY7fQ0KCS5zdDJ7ZmlsbDojNjY2NjY2O30NCjwvc3R5bGU+DQo8Zz4NCgk8cGF0aCBjbGFzcz0ic3QwIiBkPSJNMzcsNzEuNWMtOS42LDAtMTcuOC0zLjQtMjQuNC0xMC4xQzUuOCw1NC40LDIuNSw0Ni41LDIuNSwzN2MwLTkuNiwzLjQtMTcuOCwxMC4xLTI0LjQNCgkJQzE5LjIsNS45LDI3LjQsMi41LDM3LDIuNWM5LjYsMCwxNy44LDMuNCwyNC40LDEwLjFjNi44LDYuOSwxMC4xLDE0LjksMTAuMSwyNC40YzAsOS42LTMuNCwxNy44LTEwLjEsMjQuNA0KCQlDNTQuNCw2OC4yLDQ2LjUsNzEuNSwzNyw3MS41eiIvPg0KCTxwYXRoIGNsYXNzPSJzdDEiIGQ9Ik0zNyw1YzguOSwwLDE2LjQsMy4xLDIyLjYsOS40QzY1LjgsMjAuNyw2OSwyOC4xLDY5LDM3cy0zLjEsMTYuNC05LjQsMjIuNkM1My4zLDY1LjgsNDUuOSw2OSwzNyw2OQ0KCQlzLTE2LjQtMy4xLTIyLjYtOS40UzUsNDUuOSw1LDM3czMuMS0xNi40LDkuNC0yMi42QzIwLjYsOC4xLDI4LjEsNSwzNyw1IE0zNywwQzI2LjgsMCwxOCwzLjcsMTAuOSwxMC45QzMuNywxOCwwLDI2LjgsMCwzNw0KCQljMCwxMCwzLjYsMTguOCwxMC44LDI2LjFDMTcuOSw3MC4zLDI2LjgsNzQsMzcsNzRjMTAsMCwxOC44LTMuNiwyNi4xLTEwLjhDNzAuMyw1Ni4xLDc0LDQ3LjIsNzQsMzdjMC0xMC0zLjYtMTguOC0xMC44LTI2LjENCgkJQzU2LjEsMy43LDQ3LjIsMCwzNywwTDM3LDB6Ii8+DQo8L2c+DQo8cGF0aCBjbGFzcz0ic3QyIiBkPSJNNTIuNCw0Ny43TDQxLjcsMzdsMTAuOS0xMC43YzAuNi0wLjYsMC42LTEuMSwwLTEuN2wtMy4xLTNjLTAuMy0wLjMtMC42LTAuNC0wLjktMC40Yy0wLjIsMC0wLjQsMC4xLTAuNywwLjQNCglMMzcsMzIuMUwyNi4xLDIxLjZjLTAuMy0wLjMtMC41LTAuNC0wLjctMC40Yy0wLjMsMC0wLjYsMC4xLTAuOSwwLjRsLTMsM2MtMC42LDAuNi0wLjYsMS4xLDAsMS43TDMyLjQsMzdMMjEuNiw0Ny45DQoJYy0wLjIsMC4xLTAuMywwLjMtMC4zLDAuN3MwLjEsMC43LDAuMywwLjlsMywzLjFjMC4yLDAuMiwwLjUsMC4zLDAuOSwwLjNzMC43LTAuMSwwLjktMC4zTDM3LDQxLjdsMTAuOSwxMC43DQoJYzAuMywwLjMsMC41LDAuNCwwLjcsMC40YzAuMywwLDAuNi0wLjEsMC45LTAuNGwzLTNjMC4zLTAuMiwwLjQtMC41LDAuNC0wLjlDNTIuOSw0OC4zLDUyLjcsNDgsNTIuNCw0Ny43eiIvPg0KPC9zdmc+") center center no-repeat';
        el.style.backgroundSize = 'cover';
        el.style.width = "29px";
        el.style.height = "29px";
        el.innerText = ' ';
        return el;
    }

    function setCookie(cname, cvalue, exdays) {
        var d = new Date();
        d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
        var expires = "expires=" + d.toUTCString();
        document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
    }

    function getCookie(cname) {
        var name = cname + "=";
        var decodedCookie = decodeURIComponent(document.cookie);
        var ca = decodedCookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') {
                c = c.substring(1);
            }
            if (c.indexOf(name) == 0) {
                return c.substring(name.length, c.length);
            }
        }
        return "";
    }
})();
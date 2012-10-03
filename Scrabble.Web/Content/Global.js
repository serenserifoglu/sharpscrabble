String.prototype.format = function ()
{
    var pattern = /\{\d+\}/g;
    var args = arguments;
    return this.replace(pattern, function (capture)
    {
        return args[capture.match(/\d+/)];
    });
}

function createCookie(name, value, days)
{
    if (days)
    {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        var expires = "; expires=" + date.toGMTString();
    }
    else var expires = "";
    document.cookie = name + "=" + value + expires + "; path=/";
}

function readCookie(name)
{
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++)
    {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

function eraseCookie(name)
{
    createCookie(name, "", -1);
}

function gameId()
{
    return readCookie('GameId');
}

/* Game implementation */
var invoker = (function ()
{
    var methods =
    {
        DrawTurn: function (message)
        {

        },
        GameOver: function (message)
        {

        },
        NotifyTurn: function (message)
        {
            if (isCurrentPlayer(message.PlayerId))
                enableButtons();
            else
                disableButtons();
        },
        TilesUpdated: function (message)
        {
            var r = playerRack(message.PlayerId);
            r.empty();
            $.each(message.Payload, function (i, t)
            {
                r.append(makeTile(t, message.PlayerId));
            });
        },
        Debug: function () { }
    };

    return {
        handle: function (raw)
        {
            var message = JSON.parse(raw);
            if (message.What in methods)
            {
                methods[message.What](message);
            }
        }
    }
})();

function playerRack(who)
{
    return $('#player-{0} .rack'.format(who));
}

function makeTile(t, who)
{
    return $('<div>')
        .addClass(isCurrentPlayer(who) ? 'tile movable' : 'tile')
        .text(t.Letter)
        .append($('<span>').addClass('tileScore').text(t.Score));
}

function isCurrentPlayer(who)
{
    return who == 0;
}

function enableButtons()
{
    buttonArea.show();
    $('button', buttonArea).button('enable');
}

function disableButtons()
{
    $('button', buttonArea).button('disable');
}

$(document).ready(function ()
{
    buttonArea = $('#buttonArea');
    $('button', buttonArea).button();
})
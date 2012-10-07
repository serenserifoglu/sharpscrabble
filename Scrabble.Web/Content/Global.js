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
            var baseLeft = r[0].offsetLeft;
            r.empty();
            $.each(message.Payload, function (i, t)
            {
                var e = makeTile(t, message.PlayerId);
                r.append(e);
                e.css('left', baseLeft + 36 * i);
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
    var current = isCurrentPlayer(who);
    var t = $('<div>')
        .addClass(current ? 'tile movable' : 'tile')
        .text(t.Letter)
        .append($('<span>').addClass('tileScore').text(t.Score));
    return current ? movable(t) : t;
}

function positionRack(r)
{
    var baseLeft = r.offsetLeft;
    $('.tile', r).each(function (i, e)
    {
        $(e).css('left', baseLeft + 36 * i);
    });    
}

function movable(what)
{
    return what.draggable({ containment: '#playingArea', revert: 'invalid', revertDuration: 150, /*snap: '#board td', snapMode: 'inner'*/ });
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
    //playerRack(0).sortable({ axis: 'x' });
    $('#board td').droppable(
    {
        drop: function (event, ui)
        {
            $(this).removeClass('tile-over').addClass('occupied');
            console.log(event);
            console.log(ui.draggable);
        },
        over: function()
        {
            if (!$(this).hasClass('occupied'))
                $(this).addClass('tile-over');
        },
        out: function()
        {
            $(this).removeClass('tile-over');    
        },
        accept: function()
        {
            return !$(this).hasClass('occupied');    
        }
    });
    movable($('.movable'));
    $('.rack').each(function (i, r)
    {
        positionRack(r);
    });
})
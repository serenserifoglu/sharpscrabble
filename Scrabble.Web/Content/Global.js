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

function wtf()
{
    $('<div><p>An unexpected error has occured.</p><p>The page will be reloaded to attempt to continue the game.</p></div>').dialog(
    {
        title: 'Whoops',
        buttons: 
        [
            { text: "Alright :'(", click: function () { window.location.href = window.location.href; } }
        ]
    });
}

function simpleDialog(message)
{
    $('<p>').text(message).dialog(
    {
        title: 'SharpScrabble',
        buttons:
        [
            { text: 'Ok', click: function() { $(this).dialog("close"); } }
        ]
    });
}

function simpleConfirm(message, callback)
{
    $('<p>').text(message).dialog(
    {
        title: 'SharpScrabble',
        buttons:
        [
            { text: 'Ok', click: callback },
            { text: 'Cancel', click: function () { $(this).dialog("close"); } }
        ]
    });   
}

function gameId()
{
    return readCookie('GameId');
}

/* Game implementation */
var TurnTypes =
{
    Pass: 'Pass',
    DumpLetters: 'DumpLetters',
    PlaceMove: 'PlaceMove'
};

var turnMgr = (function ()
{
    var ok = false;
    var turnInput = { Type: null, Tiles: [] };

    return {
        setCurrentTurn: function ()
        {
            ok = true;
        },
        commit: function ()
        {
            if (ok && turnInput.Type)
            {
                disableButtons();
                //Post this move to the server
                $.ajax(
                {
                    type: 'POST',
                    url: '/play/{0}/taketurn'.format(gameId()),
                    dataType: 'json',
                    data: turnInput,
                    success: function (response)
                    {
                        if (response.IsValid)
                        {
                            ok = false;
                            turnInput.Type = null;
                            turnInput.Tiles = [];
                        }
                        else
                        {
                            simpleDialog('The move was not valid, please try again.');
                            reset();
                            enableButtons();
                        }
                    },
                    error: function (xhr)
                    {
                        if (xhr.status != 0)
                            wtf();
                    }
                });
            }
        },
        reset: function ()
        {
            //Needs to return any placed tiles to the player's rack.
            for (var i = 0; i < turnInput.Tiles.length; i++)
            {
                var t = turnInput.Tiles[i];
                //Do something with t.
            }
            turnInput.Type = null;
            turnInput.Tiles = [];
        },
        tileDown: function (x, y, tile)
        {
            var letter = tile.attr('data-letter');
            var index = tile.index();
            console.log('{0} placed at ({1}, {2}). Tile index = {3}'.format(letter, x, y, tile.index()));
            turnInput.Type = TurnTypes.PlaceMove;
            turnInput.Tiles[index] = { X: x, Y: y, Letter: letter };
        },
        pass: function ()
        {
            turnInput.Type = TurnTypes.Pass;
            turnInput.Tiles = [];
            commit();
        },
        dumpLetters: function ()
        {
            turnInput.Type = TurnTypes.DumpLetters;
            turnInput.Tiles = [];
            commit();
        },
        log: function ()
        {
            console.log(turnInput);
        }
    };
})();

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
            {
                turnMgr.setCurrentTurn();
                enableButtons();
            }
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
                var e = makeTile(t, isCurrentPlayer(message.PlayerId));
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

function makeTile(t, canMove)
{
    var t = $('<div>')
        .addClass(canMove ? 'tile movable' : 'tile')
        .text(t.Letter)
        .attr('data-letter', t.Letter)
        .append($('<span>').addClass('tileScore').text(t.Score));
    return canMove ? movable(t) : t;
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
    return what.draggable({ containment: '#playingArea', revert: 'invalid', revertDuration: 150, stack: '.tile' /*snap: '#board td', snapMode: 'inner'*/ });
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
    //Board setup
    buttonArea = $('#buttonArea');
    $('button', buttonArea).button();
    $('#board td').droppable(
    {
        drop: function (event, ui)
        {
            $(this).removeClass('tile-over').addClass('occupied');
            var x = $(this).index();
            var y = $(this).parent().index();
            turnMgr.tileDown(x, y, ui.draggable);
        },
        over: function ()
        {
            if (!$(this).hasClass('occupied'))
                $(this).addClass('tile-over');
        },
        out: function ()
        {
            $(this).removeClass('tile-over');
        },
        accept: function ()
        {
            return !$(this).hasClass('occupied');
        }
    });
    movable($('.movable'));
    $('.rack').each(function (i, r)
    {
        positionRack(r);
    });
    //Button clicks
    $('#done').click(function ()
    {
        turnMgr.commit();
    });
    $('#pass').click(function ()
    {
        simpleConfirm('Are you sure you want to pass?', function ()
        {
            turnMgr.pass();
        });
    });
    $('#dumpLetters').click(function ()
    {
        simpleConfirm('Are you sure you want to exchange your tiles?', function ()
        {
            turnMgr.dumpLetters();
        });
    });
    $('#reset').click(function ()
    {
        turnMgr.reset();
    });
})
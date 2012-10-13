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
        modal: true,
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
        modal: true,
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
        modal: true,
        title: 'SharpScrabble',
        buttons:
        [
            { text: 'Ok', click: function () { callback(); $(this).dialog('close'); } },
            { text: 'Cancel', click: function () { $(this).dialog('close'); } }
        ]
    });   
}

function gameId()
{
    return readCookie('GameId');
}

/* Game implementation */
var currentPlayerIndex = -1;

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
            var r = this.reset;
            if (ok && turnInput.Type)
            {
                disableButtons();
                //Make a condensed version of the data to serialize to JSON.
                var toSend = { Type: turnInput.Type, Tiles: [] }
                for (var i in turnInput.Tiles)
                {
                    var info = turnInput.Tiles[i];
                    toSend.Tiles.push({ X: info.X, Y: info.Y, Letter: info.Letter });
                }
                //Post this move to the server
                invoker.setQueueMode();
                $.ajax(
                {
                    type: 'POST',
                    url: '/play/{0}/taketurn'.format(gameId()),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: JSON.stringify(toSend),
                    success: function (response)
                    {
                        if (response.IsValid)
                        {
                            console.log('Got response from post');
                            ok = false;
                            $.each(turnInput.Tiles, function (i, info)
                            {
                                if (info)
                                    cloneTile(info.Element);
                            });
                            turnInput.Type = null;
                            turnInput.Tiles = [];
                        }
                        else
                        {
                            enableButtons();
                            simpleDialog('The move was not valid, please try again.');
                            r();
                        }
                        invoker.setImmediateMode();
                        invoker.runAll();
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
            $.each(turnInput.Tiles, function (i, t)
            {
                if (t)
                {                    
                    t.Element.data('square').removeClass('occupied');
                    t.Element.data('square', null);
                    var origin = t.Element.data('origin');
                    t.Element.animate({ left: origin.left, top: origin.top }, 150);
                }
            });
            turnInput.Type = null;
            turnInput.Tiles = [];
        },
        tileDown: function (x, y, tile)
        {
            var letter = tile.attr('data-letter');
            var index = tile.index();
            console.log('{0} placed at ({1}, {2}). Tile index = {3}'.format(letter, x, y, tile.index()));
            turnInput.Type = TurnTypes.PlaceMove;
            turnInput.Tiles[index] = { X: x, Y: y, Letter: letter, Element: tile };
        },
        pass: function ()
        {
            turnInput.Type = TurnTypes.Pass;
            turnInput.Tiles = [];
            this.commit();
        },
        dumpLetters: function ()
        {
            turnInput.Type = TurnTypes.DumpLetters;
            turnInput.Tiles = [];
            this.commit();
        },
        log: function ()
        {
            console.log(turnInput);
        }
    };
})();

var invoker = (function ()
{
    var queueMode = false;
    var queue = [];
    var methods =
    {
        DrawTurn: function (message)
        {
            var wrapped = message.Payload;
            if (wrapped.What === TurnTypes.PlaceMove)
            {
                if (message.PlayerId != currentPlayerIndex) //we already have the current player's tiles placed
                {
                    $.each(wrapped.Payload, function (i, item)
                    {
                        putTile(item.Key.X, item.Key.Y, item.Value);
                    });
                }
                updateScore(message.PlayerId, wrapped.NewScore);
            }
            //else, nothing really to do (in all cases, we update the summary)
            showSummary(wrapped.Summary);
        },
        GameOver: function (message)
        {
            showSummary('Game has ended! Finalising scores.');
            console.log(message);
            $.each(message.Payload.AllPlayers, function (i, p)
            {
                //update score and tiles.
                updateScore(p.Id, p.Score);
                refreshPlayerTiles(p.Id, p.Tiles, false);
            });
            showWinners(message.Payload.Winners);
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
            refreshPlayerTiles(message.PlayerId, message.Payload, isCurrentPlayer(message.PlayerId));
        },
        Debug: function () { }
    };

    return {
        add: function (raw)
        {
            var message = JSON.parse(raw);
            if (message.What in methods)
            {
                var fn = (function (innerMessage)
                {
                    return function () { methods[innerMessage.What](innerMessage); }
                })(message);
                if (queueMode)
                    queue.push(fn);
                else
                {
                    this.runAll();
                    fn();
                }
            }
        },
        runFirst: function ()
        {
            var first = queue.shift();
            if (first)
            {
                first();
                return true;
            }
            else
                return false;
        },
        runAll: function ()
        {
            while (this.runFirst()) { };
        },
        setQueueMode: function ()
        {
            queueMode = true;
        },
        setImmediateMode: function ()
        {
            queueMode = false;
        }
    }
})();

function playerRack(who)
{
    return $('#player-{0} .rack'.format(who));
}

function updateScore(who, value)
{
    $('#player-{0} .playerScore'.format(who)).text(value);    
}

function showSummary(value)
{
    console.log(value); 
    var cssClass = consoleContainer.children().length % 2 == 0 ? 'entry' : 'entry alt';
    consoleContainer.prepend($('<div>').addClass(cssClass).html(value));
}

function showWinners(winners)
{
    if (!winners || winners.length == 0)
    {
        console.log('winners array was empty');
        return;
    }

    var winningScore = winners[0].Score;
    var humanWin = any(winners, function (p) { return isCurrentPlayer(p.Id); });
    var hasHuman = hasHumanPlayer();
    var d = $('<div>');
    if (winners.length == 1)
    {
        if (humanWin)
        {
            $('<p>').addClass('winner').text('Congratulations {0}, you won!'.format(winners[0].Name)).appendTo(d);
            $('<p>').text('Your final score was {0}, nice job!', winningScore).appendTo(d);
        }
        else
        {
            $('<p>').addClass('winner').text('{0} has won with a final score of {1}.'.format(winners[0].Name, winningScore)).appendTo(d);
            if (hasHuman)
                $('<p>').text('Better luck next time.').appendTo(d);   
        }
    }
    else
    {
        $('<p>').addClass('winner').text(joinNames() + ' have tied with a score of {0}!'.format(winningScore)).appendTo(d);
        if (hasHuman)
        {
            if (humanWin)
                $('<p>').text("You tied the computer, that's still pretty impressive!").appendTo(d);
            else
                $('<p>').text('Better luck next time.').appendTo(d);    
        }
    }
    $('<p>').html("Check out the <a href='http://code.google.com/p/sharpscrabble/' target='_blank'>source code</a> if you're interested about how the game works!").appendTo(d);

    $(d).dialog(
    {
        modal: true,
        title: 'Game Over!',
        buttons:
        [
            { text: 'New Game', click: function () { window.location.href = '/'; } },
            { text: 'Ok', click: function () { $(this).dialog("close"); } }
        ]
    });
}

function any(array, fn)
{
    for (var i = 0, l = array.length; i < l; i++)
    {
        if (fn(array[i]))
            return true;
    }
    return false;
}

function joinNames(players)
{
    if (!players || players.length == 0)
        return '';

    if (players.length == 1)
        return players[0].Name;

    var result = '';
    var firstAdded = false;
    var last = players.length - 1;
    for (var i = 0; i < last; i++)
    {
        if (firstAdded)
            result += ', ';
        else
            firstAdded = true;
        result += players[i].Name;
    }
    result += ' and ' + players[last].Name;
    return result;
}

function refreshPlayerTiles(who, tiles, canMove)
{
    var r = playerRack(who);
    var baseLeft = r[0].offsetLeft;
    var baseTop = r[0].offsetTop;
    r.empty();
    $.each(tiles, function (i, t)
    {
        var e = makeTile(t, canMove);
        r.append(e);
        positionTile(e, baseTop, baseLeft + 36 * i);
    });
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
    var top = r.offsetTop;
    $('.tile', r).each(function (i, e)
    {
        var left = baseLeft + 36 * i;
        positionTile(e, top, left);
    });
}

function positionTile(e, top, left)
{
    $(e).css('left', left).data('origin', { left: left, top: top });    
}

function movable(what)
{
    return what.draggable({ containment: '#playingArea', revert: 'invalid', revertDuration: 150, stack: '.tile' /*snap: '#board td', snapMode: 'inner'*/ });
}

function isCurrentPlayer(who)
{
    return who == currentPlayerIndex;
}

function hasHumanPlayer()
{
    return currentPlayerIndex != -1;
}

function cloneTile(t)
{
    var clone = t.clone().removeClass('movable');
    var target = t.data('square');
    t.remove();
    appendToSquare(clone, target);
}

function putTile(x, y, tile)
{
    //console.log('Putting {0} at ({1}, {2})'.format(tile.Letter, x, y));
    var element = makeTile(tile, false);
    var square = $('td:eq(' + x + ')', '#board tr:eq(' + y + ')');
    appendToSquare(element, square);
    square.addClass('occupied');
}

function appendToSquare(e, square)
{
    $(e).appendTo(square.children().first());
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
    consoleContainer = $('#console .inner');
    $('button', buttonArea).button();
    $('#board td').droppable(
    {
        drop: function (event, ui)
        {
            $(this).removeClass('tile-over').addClass('occupied');
            var oldSquare = ui.draggable.data('square');
            if (oldSquare)
                oldSquare.removeClass('occupied');
            ui.draggable.data('square', $(this))
            var x = $(this).index();
            var y = $(this).parent().index();
            turnMgr.tileDown(x, y, ui.draggable);
            //snap to the square
            ui.draggable.position(
            {
                my: 'top left',
                at: 'to left',
                of: $(this)
            });
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
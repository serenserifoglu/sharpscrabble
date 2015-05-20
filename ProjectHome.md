# Project Hosting #

SharpScrabble has been ported to the web, and the initial version is up and running here: http://scrabble.codinghero.ws/

The web application source is in the [web branch](http://code.google.com/p/sharpscrabble/source/browse/#svn%2Fbranches%2Fweb).  Try it out, file issues, and enjoy!

# 1. Introduction #

SharpScrabble is a multiple-agent implementation of the classic board game, Scrabble, built on the .NET 4.0 Framework.  It is built in a combination of F# and C#, using the WPF graphical subsystem to develop the User Interface.  SharpScrabble can be played by 2-4 computer agents or by an individual player up against 1-3 computer agents.  Different artificial intelligence providers and utility functions can be configured for each computer agent.

# 2. About the Game of Scrabble #



Scrabble is a classic board game in which players compete by creating intersecting words on a 15x15 playing surface.  Players receive randomized tiles from a bag of tiles, from which words can be made  Players receive a score for each word summed from tile values and modifier/bonus squares on the board.

## 2.1 Game Goals and Play ##

Scrabble is played by 2-4 players and the winner (or winners) is determined by the highest score at the end of the game.  The goal of the game is to outscore all other players to be deemed the winner.

The setup phase of scrabble starts with each player first determining who goes first.  Once the order of play is confimed, each player selects random tiles from the tile bag, one at a time, rotating around the players in order of play.  When each player has 7 tiles, the game begins.

The first move must intersect the center square.  Each subsequent move must be made intersecting or adjacent to an existing word on the board.  As words are created, scores are tallied after each word is played.  A player also has the option of passing or dumping 1-7 letters and receiving new ones.  Each of these actions also sacrifices the player's current turn.

The modifier squares on the board are double and triple letter scores, as well as double and triple word scores.  A bonus is also granted to a player who uses all tiles in a single turn.

## 2.2 Common Player Strategies ##

The most common player strategy is to create the highest scoring word possible at each turn, taking into account bonus squares.  This proves to be a very effective strategy.

Another common strategy is to create multiple words by taking advantage of 2 and 3 letters that can be created perpendicularly to the direction of the word in play.  Taking advantage of the fact that pluralization is easily done with the letter S, words can be created to gain additional score.  (This is the strategy used by the SmartSMoves utility function in our implementation)

In addition to gaining extra points for multiple-word turns, 2 and 3 letter words provide a defensive type of play, which can make the next players' turns more difficult to create high scoring words.  For this reason, many Scrabble champions learn as many 2 and 3 letter words as they can.

Other strategies include playing toward bonus squares, saving common letters, managing the rack of tiles so that a player doesn't get stuck with only vowels, or playing as many 7 letter words as possible to maximize bonus points.

## 2.3 The Game Environment ##

The letter distribution in a standard game of Scrabble is as follows:

  * 2 blank tiles (scoring 0 points)
  * _1 point_: E ×12, A ×9, I ×9, O ×8, N ×6, R ×6, T ×6, L ×4, S ×4, U ×4
  * _2 points_: D ×4, G ×3
  * _3 points_: B ×2, C ×2, M ×2, P ×2
  * _4 points_: F ×2, H ×2, V ×2, W ×2, Y ×2
  * _5 points_: K ×1
  * _8 points_: J ×1, X ×1
  * _10 points_: Q ×1, Z ×1

The environment for each agent or player is partially observable.  Players do not know what tiles are possessed by other players or which tiles still remain in the tile bag.  The boards and all turns already played are fully observable on the board at all times.

Every turn in Scrabble occurs at a concrete interval of time.  As each player finishes a turn, control is passed to the next player.  Each turn is only dependent on the current state of the board.  The board is static and does not change without being acted upon by a player.

Each player knows what immediate effect their actions will have on the game environment, with the exception of dumping letters.  This creates an element of chance related to which tiles may be drawn from the tile bag.

Typically, the players in a Scrabble game are all behaving competitively.  The object of the game is to win and each player should be trying to do just that.

Technically, while there is a huge number of potential states a Scrabble game can be in, the number is finite.  There is a finite set of words that can be created, even taking blanks into account.  There is also a finite number of ways the tiles can be placed onto the boards 255-square playing surface.

# 3. The SharpScrabble Implementation #

## 3.1 Technologies and Implementation Details ##

SharpScrabble is implemented upon the Microsoft .NET 4.0 framework.  There are 3 distinct parts to the application.  First, the Dictionary provides access to the official word list to check the validity of each word played.  Next, the game engine contains the game state management, the actions that are performed to change state, and the providers for AI and utility functions.  These first two parts are built in F# 4.0, Microsoft's functional programming language.  The last piece of the implementation is the User Interface and Display, which contains methods to process user input and display intuitive representations of the game state.  The UI/Display assemblies are built in C# using Windows Presentation Foundation (WPF), a graphical subsystem for rendering Windows user interfaces.

All of these assemblies are compiled to Microsoft's IL and run in Microsoft's Common Language Interface(CLI)

When the game is setup to be played by a human player, only the human player's window is shown.  Agent windows are not shown to replicate the true play of a game of Scrabble.  When multiple agents run with no humans, all windows and all agent tiles are shown for analysis purposes.

## 3.2 The Dictionary ##

One particularly key part of the implementation of any Scrabble game is the dictionary.  Without this, there is no way for an intelligent agent to determine if a placement of tiles is valid or not.  Since an agent must access the dictionary many times in determining a move, and since the dictionary is bound to be very large, performance in accessing valid words is very important.

The dictionary we started with is the Official Tournament Word List (TWL), which is used for professional Scrabble tournaments in the USA and Canada.  Other parts of the world use a similar dictionary called Chambers Official Scrabble Words, and a merging of the two list called SOWPODS also exists.  The TWL contains approximately 179,000 English words (Official).

For performance reasons, the SharpScrabble implementation alters the way the dictionary is stored in memory, choosing not to keep it as a simple list.  The data structure used is a map from string to a list of strings.  The key in this structure is an alphabetized string and the value is a list of valid words that can be formed by taking any permutation of these letters.  Note that this does not include permutations of different sized combinations of these letters, but merely permutations of all given letters.  For example, the value corresponding with key “aet” would be a list containing “eat”, “ate”, and “tea”, but not “at.”

The motivation for storing the data in this particular structure will become more obvious when the artificial intelligence strategies are discussed below.  In a nutshell, this structure allows an agent to retrieve a list of all possible words of a given size from a given set of letters in constant time.  This saves iterating a list of words doing string comparisons and also the cost of calculating the permutations of a string.

## 3.3 AI Strategies Employed ##

The SharpScrabble implementation has three different modes of artificial intelligence strategies that an agent can use.  Each of these performs a search across at least a portion of the total game space, and determines a move to take based off of the outcome of a given utility function.  These functions will be discussed in the next section, but for now we can assume that each utility function will be given a move to evaluate and will return a real number denoting the relative value of that move.  The highest scoring move that the search algorithm finds is the move that will be played.

The first strategy that was written is probably the most obvious – brute force.  In the case of Scrabble, a brute force algorithm is one which will evaluate each possible valid move to find the best score (for the remainder of this section, score will refer to the output of a given utility function, and not necessarily the score that the player will receive from the actually play).

The search through all possible valid moves goes as follows.  If this is the first move in the game, the only constraint is that a tile must be played on the center square.  In this case, the algorithm finds all valid words that can be made by the tiles in the agents' hands.  The algorithm then determines all of the ways that each word could be placed on the board such that it has one letter on the center tile. Each of these moves is evaluated to determine its score, and the best score is played.  If the move is not the first in the game, the strategy is very similar, but somewhat more complex.  The constraints in this case are that the word must utilize some tile that is already currently on the board, and that no invalid words in any direction can be formed as a result of the move being played.  To determine any move that is not the first, the algorithm iterates through all of the tiles that are currently played on the board and performs the same search for each of them as the first move does on the center tile.  That is, all valid words based on the agent's tiles and the given tile on the board are found, as well as all possible positions that it can be placed on the board at the given spot without creating an invalid move.  Of all words played in all valid configurations at all valid spots on the board, the maximum score is chosen and played.  As is obvious, this is guaranteed to find the best possible outcome for the given board and set of input tiles, since all possibilities are searched.

The second strategy that we wrote was a hill-climbing algorithm.  Hill-climbing algorithms are a standard set of strategies that are common in situations where a brute-force search is simply infeasible.  The idea is to choose a random point in the game space and perform a localized search until some maximum value is found, e.g. while the scores are walking up a hill.  Once the top of the hill, or local maximum, has been found, the algorithm chooses that as the answer and returns it.  This saves time over a brute-force approach since in most cases only a fraction of all options will be evaluated, but it is still a better strategy than choosing a random move since it is known to be a better option than some other move.

The main premise of our hill-climbing implementation is the same as the brute force implementation.  Before iterating the set of tiles on the board, the set is ordered randomly so we have random starting point.  The algorithm then proceeds as in the brute-force method, but keeping track of the best score seen so far.  As long as the next score is as good as or better than the best one seen so far, the search continues.  However as soon as the next score is worse than the best seen so far, the best seen move is chosen as the answer.

Intuitively, a simple hill-climbing algorithm is not an extremely good algorithm, especially when the landscape of possible scores is likely to be volatile, as is the case in Scrabble.  In these situations, there are a lot of small local maximum scores, and it is likely that one of these will be chosen instead of the global maximum or something close to it.  One answer to this is to allow your hill-climbing algorithm to restart at another random point on the landscape some number of times.  This is referred to as random-restart hill-climbing, and is the third strategy that we developed.

A random-restart hill-climber performs just like a hill-climber, except it will retry several times.  Our implementation takes the number of times to retry as a parameter.  Once a local maximum has been found, the algorithm reorders the set of tiles randomly again and starts over, decrementing the number of restart tries.  Through this, the best overall score is remembered.  At the end of all ''n'' retry attempts, the best score out of all of the hill-climbing runs is selected and played.  We can see that this approach is likely to improve a simple hill-climbing algorithm by using the power of probability.  With a single random placement, it might be unlikely that the global maximum or a very good local maximum is nearby; however with ten or twenty random placements on the landscape this probability increases dramatically.

## 3.4 Utility Functions ##

The Scrabble AI is composed of two parts.  The first is the move generator, which uses one of the three strategies discussed above.  The second part of the AI is the utility functions, which determine how effective a given move will be.  Most popular Scrabble artificial intelligence implementations factor in the score of the move with the usefulness of the rack, which is the player's left over tiles.  Our implementation consists of a number of different utility functions, which aim to improve on simply taking the best move at a given time.  The utility functions are: MaximumScore, SaveCommon, SmartSMoves, and UseBonusSquares.  Each utility function has the same input and output parameters.  Each function takes in the player's proposed move (which is a map from board coordinate to tile), along with their tiles, so we can compute the set of tiles that are left over after playing the move.  Each function will return a floating point which represents the move's overall value.

The MaximumScore function is simple, it computes the actual score of the move and returns it.  This is the basis for all other utility functions to improve on.  All of the other utility functions will compute the move's base score, and then adjust it based on other factors.

The SaveCommon utility function will return higher values when the move does not play “common tiles.”  A common tile is a letter than is found very frequently in the English language, and hence can be used to form longer words.  In the game of Scrabble, when a player makes a move that uses all 7 of his or her letters, a 50 point bonus is awarded.  This is casually known as making a “bingo,” and world class scrabble players typically make 2 or 3 of these per game.  The common tiles that the utility functions checks for are A, E, I, N, R and S.  The utility function will return a higher value for moves which leave these letters on the player's rack.  The computation used in the function is simply the move's base score, plus 5 for each common tile left on the player's rack.  The idea behind this function is that making the best possible move each time can degrade the overall score, and that saving for the 50 point bonus can make up for not choosing the best possible move at a given point in time.

The SmartSMoves utility function builds on the concept that a S tile can be used to form two words at once.  With an S, a player can make an existing word on the board plural, and build another word that uses that S.  The end result is that moves that do not form two words with a S will be scored lower than the move's base score.  Thus, the computer will be more likely to save the S tiles it has, and use them for higher scoring moves later in the game.

The last utility function is a defensive function.  Like its name suggests,  UseBonusSquares will rank moves that occupy bonus squares higher than those that do not.  The idea is that by using a bonus square (such as a double letter score or a double word score square), you are taking away potential points that an opponent could gain from using the square.  To factor in what an opponent might gain from a bonus square, we use the knowledge that the average Scrabble tile carries a score of 1.9.  Thus, if a move occupies a double letter score square, the utility function will add that 1.9 points that the opponents will not get, and likewise for a triple letter score square.  For the double and triple word score squares, we used the heuristic that an average Scrabble play consists of 3.5 tiles, so the overall bonus of a double word score would be 3.5 x 1.9 = 6.65.  Thus, when a potential move occupies a double word score, its utility is the move's base score plus 6.65.  Likewise for a triple word score square, the move's score would be the base score plus 13.3.

# 4. Implementation Analysis #

After implementing our various agent strategies and utility functions, we wanted to simulate a number of games of different agents playing each other for a few reasons.  First, this is a good test for our code since playing many games will likely exercise most if not all portions of our code base, and therefore allow us to find bugs or cases that we hadn't considered previously.  Second, it would serve to validate our strategies and functions; for example, we know that a brute-force search strategy should be unbeatable, and that adding random-restarting to a hill climbing technique should improve the strategy.  Running many simulations of different agents would allow us to verify that our implementations were running according to these assumptions.  Lastly, it would allow us to study which strategies (utility functions) are actually most useful in Scrabble, at least when playing an intelligent agent, and what impact the different functions would have on game time and other items of interest.

For a graphical representation of the results discussed below, see Sections 7.1 and 7.2, respectively.

## 4.1 Search Strategy Analysis ##

Our analysis of different agent search strategies consisted of setting up different agents to play each other in a set of 300 games.  The end of a game was defined as either one player running out of tiles, or both players passing two times in a row when the bag of tiles was empty (and therefore swapping tiles was not feasible).  As a control group, we first had two hill-climbing agents play against each other.  The results were within a reasonable tolerance of what was expected, with one agent winning 53% of the time and the other agent winning the other 47% of the time; there were no ties.

Seeing that our setup seemed reasonable for more testing, we then began running other simulations, continuing up the chain of more intelligent agents the whole way.  The other simulations were, in order of increasing agent complexity; Hill-Climbing vs Random Restart (3 restarts) Hill-Climbing; Random Restart (3) vs Random Restart (5); Random Restart (5) vs Brute Force; and Random-Restart (15) vs Brute Force.  The overall scores of these simulations was unsurprising; Brute Force is the best strategy, simple Hill Climbing will likely be beaten by anything using a random restart, and a higher number of restarts will achieve a higher score than a lower number.  Some strategies dominate each other more than others; for example Brute Force will win 99% of the time over any of the other search strategies we tested, but Random-Restart (3) will only beat Random-Restart (5) some 81% of the time.

However other measured metrics of the results are more interesting.  For example, the total playing time for each game was recorded.  As we can see from the graph in section 7.1, as the complexity of the search went up, so did the total game time.  Two hill-climbing agents playing each other can finish a game on an average of one second, where a brute-force agent playing a random-restart (5) agent will take 8 seconds.  The number of restarts that a random-restart agent takes clearly has a negative impact on the amount of time to determine a strategy.

Analyzing the number of moves in a game is also interesting.  The more complex the search strategy is, the fewer moves a game will consist of.  This is because the smarter agents are finding more complex words, which leads to using more tiles at each turn and therefore covering more of the board.

One more piece of information that can be deduced from the results is that the number of random restarts an agent would need in order to come close to being on par with a brute force agent would be very large, and also very slow.  The random-restart (15) agent still lost to the brute force agent the same percent of times as the random-restart (5) agent; the only benefit of the extra ten restarts was that the agent was beaten by about 40 points less on average.  However this was at a cost of 50% more time to play a game.  Assuming a similar progression follows, it would take 45-restart agent, and double the 5-restart agent's time, to be good enough to come within 100 points of the brute-force strategy on average.

## 4.2 Utility Function Analysis ##

Once tests had been performed on the search strategies, it was time to determine which utility function was the best at determining the weight of a move.  Since we had already shown that two hill-climbing agents would perform approximately the same over a period of 300 games, and because these were the fastest-playing agents, we used two hill-climbers against each other in this analysis.

Using the basis that a standard utility function would simply use the score of the move as it is played on the board, unweighted by anything else, our Maximum Score function was used as the control, and each other function was played against it.  The first five tests in the graph in section 7.2 are arranged in order of increasing success vs the maximum score strategy.  In order, the usefulness of our utility functions turns out to be: Maximum Score; Smart S Moves; Use Bonus; a combination of Smart S Moves, Use Bonus, and Save Common; a combination of Smart S and Save Common; and lastly Save Common.  From this we can see that trying to save common letters in hopes of forming a 7-letter word and getting the 50-point bonus is a futile strategy, while saving S tiles until they can be played is probably worthwhile.  In addition, defensive strategies such as playing bonus tiles all of the time so your opponent cannot do not appear to be worthwhile either.

The last two bars on the chart cement that Smart S moves is the best of our three non-maximum score strategies, outperforming Use Bonus 53% of the time and Save Common 66% of the time.  However all of this analysis must be taken with the following in consideration.  Since our agents know of all possible playable words, the effectiveness of these strategies may be different in a real-world environment in which each agent has a limited knowledge of the dictionary.  In order to test this effectively we would need to determine different levels of knowledge and restrict our agents to certain sets of words based on this.  Even an expert player may only know of half of the words present in the dictionary, and make some mistakes when determining which words can be made at a certain spot on the board.

# 5. Conclusion #

## 5.1 Other Games Considered ##

In the beginning of the semester, we debated several other games of chance or full information.  The first game we considered was Poker, specifically Texas Hold 'em.  While Poker is a game firmly rooted in probability, it requires an extensive amount of learning and player evaluation to be successful.  This is something that proves to be our of scope for a single semester project.  Other ideas included Chess, which has been done so many times it wouldn't have been too interesting, Stratego, which has a huge branching factor given the start state of the game.  In Stratego, each player has 40 pieces that can be placed arbitrarily in a 4 x 10 board.  We also considered implementing a much simpler game, such as Connect Four, but ultimately we decided this was too simple, and the demo would be less than impressive.  Connect Four is a provable solvable game, and we felt that a game with an element of chance, such as Scrabble, would be an interesting challenge.  Scrabble as a game is more open ended to different strategies when compared to a fully observable game like Connect Four.  When the entire game tree can be computed, it removes a bit of fun from the game, which we wanted to preserve in our project.

## 5.2 Lessons Learned ##

The team took this opportunity to learn a programming language that was new to all of us, F#.  Given than F# compiles into the same intermediate code as our main language, C#, we felt choosing F# would give us an interesting challenge, while preserving our ability to work with C# on certain areas of the project (which was the GUI).  Since F# is a first class .NET language (Microsoft's run-time environment), we can write code in F# and C# and they will interoperate seamlessly.  In addition, F# is a functional language, which we felt would lend itself very nicely towards computational based programming, such as brute force combination generation.

Looking back on this decision, we are all very proud of what we accomplished and how much we learned about functional programming.  The three of us all work with object oriented languages for a living, so working on a project like this proved to be an extremely beneficial experience.  There are many things that can be learned about a given programming language by reading tutorials and doing small exercises, but most of the subtleties, complexities, benefits, and tradeoffs of a language do not reveal themselves until it's time to implement a real application.

Aside from learning a new programming language, we also learned a few key lessons about managing a project and utilizing our time effectively.  The first half of the semester, the team focused on two things, learning F# and implementing a human based Scrabble game. It wasn't until the second half of the semester until the team started to implement the artificial intelligence, utility functions, and simulating games to compare the results of our AI code.  Since we were bounded by only one semester, we were not able to implement more advanced features for our artificial intelligence.  For example, some of the most effective AI implementations for Scrabble compute what their opponent(s) are likely to be holding for tiles in the end game.  Then, the AI can attempt to play defensively, that is, use tiles such that their opponent can not make a move (Sackley).  One of the best Scrabble AI implementations, called Maven, divides the game into three phases, and towards the endgame, it computes the game tree, because the opponents tiles are known at that point.  Maven also uses rack evaluation, which determines relative value to the tiles that a player holds.  Thus, when determining the value of a possible move, the AI factors in how useful the leftover tiles are.  This is very important, because poor AI might use a high scoring word, while leaving itself with say only consonants, where it would be difficult to play the next move (Maven).

Because our time for this project was limited, we were not able to implement these types of advanced features.  Potential improvements to the AI would include rack evaluation, where our utility function would determine relative ranking of left over Scrabble tiles and factor that into the decision for choosing the best move.  In addition, end game logic would benefit our AI greatly.  While the opening and mid game of Scrabble is only partially observable, the end game is fully observable, because the letter frequencies are known by all players.  Even further advanced AI would simulate drawing tiles from the bag during the mid game.

Since the team spent a great deal of time implementing the Scrabble rule engine and game state, a more practical approach would have been utilizing an existing, open source, implementation of Scrabble, and just focus on developing the AI.  There are many open source implementations of Scrabble, such as the popular Quackle implementation.  Many developers write AI for Quackle, and this would have been a great opportunity to compare our implementation with others.  Overall, implementing our own game engine proved to be an excellent experience though.

## 5.3 SharpScrabble Future Enhancements ##

Of course, there is room for improvement to the SharpScrabble.  This is an outline of several improvements that we would have liked to include, but had to omit in the interest of time.

Additional providers and utility functions are easily added, but we were only able to implement a handful of each.  Future enhancements would be additional providers, such as a Simulated Annealing provider, a Genetic algorithm style provider, or local beam search.  Currently, only an exhaustive search and two implementations of Hill Climbing, including a parametrized random-restart.

Currently, only one utility function can be applied at a time.  A future enhancement that was considered was to allow weighted combinations of utility functions.  This would allow further analysis of effective strategies as well as effective combinations of any combination of strategies.  For example, MaximumScore could be weighted 0.8, and SmartSMoves 0.2, creating a brand new strategy for a player.

The current implementation does not include blanks, which are necessary for a complete version of Scrabble.  Implementing blanks adds additional complexity to word search, but would be a manageable addition.

The user interface does not contain a configuration menu to manage game set up.  We wanted to create an initial screen with options for number of players, each agent's configuration, and other configuration modifications.

Extending the User Interface to allow more than one networked human player in a single game would be a reasonable addition, but was not included in the initial version.  A web-enabled version could also be created to run in Silverlight with little modification.

# 6. Works Cited #

Sackley, Kristen. “Winning Computer Program Created by Graduate Student Beats World Champion Scrabble Player | The Daily Illini.” _The Daily Illini | The Independent Student Newspaper at the University of Illinois since 1871_. Web. 09 Dec. 2010. <[http://www.dailyillini.com/news/2007/02/28/winning-computer-program-created-by-graduate-](http://www.dailyillini.com/news/2007/02/28/winning-computer-program-created-by-graduate-student-beats-world-champion-scrabble-player)[student-beats-world-champion-scrabble-player](http://www.dailyillini.com/news/2007/02/28/winning-computer-program-created-by-graduate-student-beats-world-champion-scrabble-player)>.

Wikipedia Contributors. “Maven (Scrabble).” _Wikipedia, the Free Encyclopedia_. Wikipedia, The Free Encyclopedia., 12 Nov. 2010. Web. 09 Dec. 2010. <[http://en.wikipedia.org/wiki/Maven\_(Scrabble](http://en.wikipedia.org/wiki/Maven_(Scrabble))>.

Wikipedia Contributors. “Official Tournament and Club word List”. _Wikipedia, the Free Encyclopedia_. Wikipedia, The Free Encyclopedia., 24 Nov. 2010. Web. 10 Dec. 2010. <[http://en.wikipedia.org/wiki/Official\_Tournament\_and\_Club\_Word\_List](http://en.wikipedia.org/wiki/Official_Tournament_and_Club_Word_List)>.


# 7. Appendix #

## 7.1 AI Strategy Comparison Graph ##
![http://sharpscrabble.googlecode.com/svn/trunk/docs/Images/max%20score%20utility%20function.png](http://sharpscrabble.googlecode.com/svn/trunk/docs/Images/max%20score%20utility%20function.png)
## 7.2 AI Utility Function Comparison Graph ##
![http://sharpscrabble.googlecode.com/svn/trunk/docs/Images/utility%20function%20comparisions.png](http://sharpscrabble.googlecode.com/svn/trunk/docs/Images/utility%20function%20comparisions.png)
\ Episode 010 finished source. Load pinned forth.fs first.
30 constant map-width
12 constant map-height
map-width map-height * constant map-area
1 constant required-relics
100 constant starting-oil
35 constant wall-char
46 constant floor-char
42 constant relic-char
62 constant exit-char
64 constant hero-char
0 constant playing
1 constant escaped
3 constant lantern-out
4 constant quit
0 constant no-message
1 constant found-relic
2 constant sealed-exit
5 constant blocked
6 constant last-relic
: wall wall-char emit ;
: floor floor-char emit ;
: hero hero-char emit ;
: relic relic-char emit ;
: exit-door exit-char emit ;
: title ." LANTERN KEEP" ;
variable player-x
variable player-y
variable oil
variable relics-found
variable game-state
variable spent
variable message
variable nx
variable ny
variable row
variable column
: require-map-size ( flag -- )
  0= if ." Invalid map row count." cr bye then
;
: row, ( addr length -- )
  dup map-width = if
    0 ?do dup i + c@ c, loop drop
  else 2drop ." Invalid map row width." cr bye then
;
\ Immutable terrain; actors live separately. Edit these text rows to make a level.
create original-map
s" ##############################" row,
s" #............................#" row,
s" #..*....#...........#........#" row,
s" #.......#...........#........#" row,
s" #.......#...........#........#" row,
s" #...................#........#" row,
s" #.......#######.#####........#" row,
s" #.......#...........#........#" row,
s" #.......#....................#" row,
s" #.......#...........#......>.#" row,
s" #............................#" row,
s" ##############################" row,
here original-map - map-area = require-map-size
create game-map map-area allot
: relic-score ( n -- points ) 100 * ;
: total-score ( relics oil -- points ) swap relic-score + ;
: score ( -- points ) relics-found @ oil @ total-score ;
: tile-address ( x y -- addr ) map-width * + game-map + ;
: in-range? ( n limit -- flag ) over > swap 0 < 0= and ;
: inside? ( x y -- flag )
  map-height in-range? swap map-width in-range? and
;
: copy-map ( -- )
  map-area 0 ?do original-map i + c@ game-map i + c! loop
;
: new-game ( -- )
  copy-map 2 player-x ! 9 player-y !
  starting-oil oil !
  0 relics-found ! playing game-state ! 0 spent !
  no-message message !
;
: check-exit ( -- )
  player-x @ player-y @ tile-address c@ exit-char = if
    relics-found @ required-relics < 0= if
      escaped game-state !
    else sealed-exit message ! then
  then
;
: pickup ( -- )
  player-x @ player-y @ tile-address c@ relic-char = if
    floor-char player-x @ player-y @ tile-address c!
    relics-found @ 1 + relics-found ! found-relic message !
    relics-found @ required-relics < 0= if last-relic message ! then
  then
;
: player-step ( dx dy -- )
  player-y @ + ny ! player-x @ + nx !
  nx @ ny @ inside? if
    nx @ ny @ tile-address c@ wall-char = if blocked message ! else
      nx @ player-x ! ny @ player-y ! -1 spent ! pickup check-exit
    then
  else blocked message ! then
;
: lower ( char -- char )
  dup 64 > over 91 < and if 32 + then
;
: command ( char -- )
  lower
  0 spent ! no-message message !
  dup 97 = if -1 0 player-step then
  dup 100 = if 1 0 player-step then
  dup 119 = if 0 -1 player-step then
  dup 115 = if 0 1 player-step then
  113 = if quit game-state ! then
;
: end-turn ( -- )
  oil @ 1 - oil ! oil @ 0= if lantern-out game-state ! then
;
: turn ( char -- )
  game-state @ 0= if
    command
    spent @ game-state @ 0= and if
      end-turn
    then
  else drop then
;
: draw-row ( y -- )
  row !
  map-width 0 ?do
    i player-x @ = row @ player-y @ = and if hero-char emit else
      i row @ tile-address c@ emit
    then
  loop cr
;
: draw ( -- )
  clrscr title cr
  map-height 0 ?do i draw-row loop
  ." Oil: " oil @ . ." Relics: " relics-found @ . cr
  ." W/A/S/D move | Q quit" cr
  game-state @ escaped = if ." ESCAPED! Score: " score . cr then
  game-state @ lantern-out = if ." YOUR LANTERN WENT OUT." cr then
  game-state @ 0 > game-state @ quit < and if ." R restart | Q quit" cr then
;
: session-key ( char -- )
  lower
  game-state @ 0= if turn else
    dup 114 = if new-game clrscr then
    113 = if quit game-state ! then
  then
;
: play ( -- )
  game-state @ quit = if new-game then
  begin draw key session-key game-state @ quit = until
  ." Back at Forth. play starts a new run after quit." cr
;
new-game
." Lantern Keep loaded. Type play to begin." cr

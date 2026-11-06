\ Episode 009 finished source. Load pinned forth.fs first.
\ Tile words deliberately use literal character codes in our first definition lesson.
: wall 35 emit ;
: floor 46 emit ;
: hero 64 emit ;
: relic 42 emit ;
: exit-door 62 emit ;
: corridor wall relic floor floor floor hero floor floor floor exit-door wall ;
: title ." Lantern Corridor" ;
: relic-score ( n -- points ) 100 * ;
: total-score ( relics oil -- points ) swap relic-score + ;
9 constant corridor-length
0 constant relic-position
8 constant exit-position
20 constant starting-oil
1 constant required-relics
variable position
variable relics-found
variable oil
0 constant playing
1 constant escaped
3 constant lantern-out
4 constant quit
variable game-state
variable spent
: new-game ( -- )
  4 position ! 0 relics-found ! starting-oil oil !
  playing game-state ! 0 spent !
;
new-game
\ 0 playing, 1 won, 3 out of oil, 4 quit.
: on-relic? ( -- flag ) position @ relic-position = ;
: exit-open? ( -- flag ) relics-found @ required-relics < 0= ;
: lantern-lit? ( -- flag ) oil @ 0 > ;
: report-exit ( -- ) exit-open? if ." Open" else ." Sealed" then ;
: pickup ( -- )
  on-relic? if relics-found @ 0= if 1 relics-found ! then then
;
: check-exit ( -- )
  position @ exit-position = exit-open? and if escaped game-state ! then
;
variable candidate
: lower ( c -- c ) dup 64 > over 91 < and if 32 + then ;
: step ( delta -- )
  position @ + candidate !
  candidate @ 0 < 0= candidate @ corridor-length < and if
    candidate @ position ! -1 spent ! pickup check-exit
  then
;
: command ( char -- )
  lower 0 spent !
  dup 97 = if -1 step then
  dup 100 = if 1 step then
  113 = if quit game-state ! then
;
: end-turn ( -- )
  oil @ 1 - oil ! lantern-lit? 0= if lantern-out game-state ! then
;
: turn ( char -- )
  game-state @ 0= if
    command spent @ game-state @ 0= and if end-turn then
  else drop then
;
variable column
: slot ( n -- )
  column !
  column @ position @ = if hero else
    column @ relic-position = relics-found @ 0= and if relic else
      column @ exit-position = if exit-door else floor then
    then
  then
;
: draw ( -- )
  title cr wall corridor-length 0 ?do i slot loop wall cr
  ." Oil: " oil @ . ." Relics: " relics-found @ . cr
  ." A/D move | Q quit" cr
  game-state @ escaped = if ." ESCAPED! Score: " relics-found @ oil @ total-score . cr then
  game-state @ lantern-out = if ." YOUR LANTERN WENT OUT." cr then
  game-state @ 0 > game-state @ quit < and if ." R restart | Q quit" cr then
;
: session-key ( char -- )
  lower game-state @ 0= if turn else
    dup 114 = if new-game then
    113 = if quit game-state ! then
  then
;
: play ( -- )
  game-state @ quit = if new-game then
  begin draw key session-key game-state @ quit = until
  ." Back at Forth." cr
;
30 constant map-width
12 constant map-height
map-width map-height * constant map-area
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
: tile-address ( x y -- addr ) map-width * + game-map + ;
: copy-map ( -- ) map-area 0 ?do original-map i + c@ game-map i + c! loop ;
variable map-row
: print-row ( y -- ) map-row ! map-width 0 ?do i map-row @ tile-address c@ emit loop cr ;
: draw-map ( -- ) map-height 0 ?do i print-row loop ;
copy-map

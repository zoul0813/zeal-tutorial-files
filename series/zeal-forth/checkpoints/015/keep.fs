\ Episode 015 finished source. Load pinned forth.fs first.
\ Lantern Keep. Load the unmodified pinned forth.fs before this file.
\ Public API: new-game ( -- ), turn ( char -- ), draw ( -- ), play ( -- ).
\ State: 0 playing, 1 escaped, 2 defeated, 3 out of oil, 4 quit.
30 constant map-width
12 constant map-height
map-width map-height * constant map-area
3 constant required-relics
100 constant starting-oil
12 constant starting-health
3 constant guard-capacity
35 constant wall-char
46 constant floor-char
42 constant relic-char
62 constant exit-char
64 constant hero-char
0 constant playing
1 constant escaped
2 constant defeated
3 constant lantern-out
4 constant quit
0 constant north
1 constant east
2 constant south
3 constant west
0 constant no-message
1 constant found-relic
2 constant sealed-exit
3 constant struck-guard
4 constant guard-hit
5 constant blocked
6 constant last-relic
create guard-x-data guard-capacity cells allot
: guard-x ( index -- addr ) cells guard-x-data + ;
create guard-y-data guard-capacity cells allot
: guard-y ( index -- addr ) cells guard-y-data + ;
create guard-hp-data guard-capacity cells allot
: guard-hp ( index -- addr ) cells guard-hp-data + ;
create guard-dir-data guard-capacity cells allot
: guard-dir ( index -- addr ) cells guard-dir-data + ;
create guard-kind-data guard-capacity cells allot
: guard-kind ( index -- addr ) cells guard-kind-data + ;
variable guard-count
: wall wall-char emit ;
: floor floor-char emit ;
: hero hero-char emit ;
: relic relic-char emit ;
: exit-door exit-char emit ;
: title ." LANTERN KEEP" ;
variable player-x
variable player-y
variable health
variable oil
variable relics-found
variable game-state
variable spent
variable message
variable guard-message
\ nx/ny are shared scratch: player and guards resolve sequentially, never concurrently.
variable nx
variable ny
variable active-guard
variable probe-x
variable probe-y
variable found-guard
variable row
variable column
variable glyph
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
s" #..*....#...........#.....*..#" row,
s" #.......#...........#........#" row,
s" #.......#...........#........#" row,
s" #...................#........#" row,
s" #.......#######.#####........#" row,
s" #.......#......*....#........#" row,
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
: guard-at ( x y -- index|-1 )
  probe-y ! probe-x ! -1 found-guard !
  guard-count @ 0 ?do
    i guard-hp @ 0 > if
      i guard-x @ probe-x @ = i guard-y @ probe-y @ = and if
        i found-guard !
      then
    then
  loop
  found-guard @
;
: new-guards ( -- )
  guard-capacity guard-count !
  10 0 guard-x ! 2 0 guard-y ! east 0 guard-dir ! 0 0 guard-kind !
  22 1 guard-x ! 7 1 guard-y ! west 1 guard-dir ! 0 1 guard-kind !
  15 2 guard-x ! 9 2 guard-y ! north 2 guard-dir ! 0 2 guard-kind !
  guard-capacity 0 ?do
    2 i guard-hp !
  loop
;
: new-game ( -- )
  copy-map new-guards
  2 player-x ! 9 player-y !
  starting-health health ! starting-oil oil !
  0 relics-found ! playing game-state ! 0 spent ! no-message message ! no-message guard-message !
  12345 seed !
;
: hurt-player ( damage -- )
  health @ swap - dup 0 < if drop 0 then health !
  guard-hit guard-message !
  health @ 0= if defeated game-state ! then
;
: strike ( guard -- )
  dup guard-hp @ 2 - dup 0 < if drop 0 then
  swap guard-hp ! struck-guard message ! -1 spent !
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
      nx @ ny @ guard-at dup 0 < if
        drop nx @ player-x ! ny @ player-y ! -1 spent ! pickup check-exit
      else strike then
    then
  else blocked message ! then
;
: lower ( char -- char )
  dup 64 > over 91 < and if 32 + then
;
: command ( char -- )
  lower
  0 spent ! no-message message ! no-message guard-message !
  dup 97 = if -1 0 player-step then
  dup 100 = if 1 0 player-step then
  dup 119 = if 0 -1 player-step then
  dup 115 = if 0 1 player-step then
  113 = if quit game-state ! then
;
: guard-adjacent? ( -- flag )
  active-guard @ guard-x @ player-x @ - abs
  active-guard @ guard-y @ player-y @ - abs + 1 =
;
: guard-candidate ( -- )
  active-guard @ guard-x @ nx ! active-guard @ guard-y @ ny !
  active-guard @ guard-dir @
  dup north = if ny @ 1 - ny ! then
  dup east = if nx @ 1 + nx ! then
  dup south = if ny @ 1 + ny ! then
  west = if nx @ 1 - nx ! then
;
: guard-can-step? ( -- flag )
  \ Default false; replace it only after all bounds, terrain and occupancy checks.
  0
  nx @ ny @ inside? if
    nx @ ny @ tile-address c@ floor-char = if
      nx @ player-x @ = ny @ player-y @ = and 0= if
        drop nx @ ny @ guard-at 0 <
      then
    then
  then
;
: commit-guard ( -- )
  nx @ active-guard @ guard-x ! ny @ active-guard @ guard-y !
;
: patrol ( -- )
  guard-candidate guard-can-step? if commit-guard else
    rand16 4 umod active-guard @ guard-dir !
    guard-candidate guard-can-step? if commit-guard then
  then
;
: guard-turn ( -- )
  guard-adjacent? if
    1 hurt-player
  else patrol then
;
: guards-turn ( -- )
  guard-count @ 0 ?do
    game-state @ 0= i guard-hp @ 0 > and if
      i active-guard ! guard-turn
    then
  loop
;
: end-turn ( -- )
  guards-turn
  game-state @ 0= if
    oil @ 1 - oil ! oil @ 0= if lantern-out game-state ! then
  then
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
      i row @ guard-at dup 0 < if drop i row @ tile-address c@ emit else drop 103 emit then
    then
  loop cr
;
: draw ( -- )
  clrscr title cr
  map-height 0 ?do i draw-row loop
  ." Health: " health @ . ." Oil: " oil @ . ." Relics: " relics-found @ . cr
  ." W/A/S/D move | Q quit" cr
  game-state @ escaped = if ." ESCAPED! Score: " score . cr then
  game-state @ lantern-out = if ." YOUR LANTERN WENT OUT." cr then
  game-state @ defeated = if ." DEFEATED." cr then
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

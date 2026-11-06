\ Episode 017 finished source. Load pinned forth.fs first.
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
: cell-array ( count "name" -- ) create cells allot does> swap cells + ;
guard-capacity cell-array guard-x
guard-capacity cell-array guard-y
guard-capacity cell-array guard-hp
guard-capacity cell-array guard-dir
guard-capacity cell-array guard-kind
\ This pin's comma advances three bytes; allocate packed tables and use ! explicitly.
create kind-health 2 cells allot
2 kind-health ! 4 kind-health 1 cells + !
create kind-damage 2 cells allot
1 kind-damage ! 2 kind-damage 1 cells + !
create kind-glyph 2 cells allot
103 kind-glyph ! 71 kind-glyph 1 cells + !
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
variable best-score
0 best-score !
\ nx/ny are shared scratch: player and guards resolve sequentially, never concurrently.
variable nx
variable ny
variable active-guard
variable probe-x
variable probe-y
variable found-guard
variable screen-ok
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
  15 2 guard-x ! 9 2 guard-y ! north 2 guard-dir ! 1 2 guard-kind !
  guard-capacity 0 ?do
    i guard-kind @ cells kind-health + @ i guard-hp !
  loop
;
: new-game ( -- )
  copy-map new-guards
  2 player-x ! 9 player-y !
  starting-health health ! starting-oil oil !
  0 relics-found ! playing game-state ! 0 spent ! no-message message ! no-message guard-message !
  12345 seed !
;
: kind@ ( guard table -- value ) swap guard-kind @ cells + @ ;
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
      score best-score @ > if score best-score ! then
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
    active-guard @ kind-damage kind@ hurt-player
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
: color-for ( char -- color )
  dup wall-char = if drop 8 else
    dup relic-char = if drop 14 else
      dup hero-char = if drop 15 else
        dup 103 = over 71 = or if drop 12 else
          exit-char = if 10 else 7 then
        then
      then
    then
  then
;
: ink-for ( char -- ) color-for 0 swap set-colors ;
: draw-row ( y -- )
  0 swap tile-address map-width type cr
;
: at-glyph ( char x y -- ) set-cursor dup ink-for emit ;
: draw-items ( -- )
  map-area 0 ?do
    game-map i + c@ dup relic-char = over exit-char = or if
      i map-width mod i map-width / 1 + at-glyph
    else drop then
  loop
  guard-count @ 0 ?do
    i guard-hp @ 0 > if
      i kind-glyph kind@ i guard-x @ i guard-y @ 1 + at-glyph
    then
  loop
  hero-char player-x @ player-y @ 1 + at-glyph
;
: draw-message ( -- )
  message @ found-relic = if ." A relic! Find the others." then
  message @ last-relic = if ." All three relics! Find the exit." then
  message @ sealed-exit = if ." Sealed: collect all three relics." then
  message @ struck-guard = if ." You strike the guard." then
  message @ blocked = if ." Stone blocks your path." then
  0 17 set-cursor
  guard-message @ guard-hit = if ." The guard hits you!" then
;
: clear-line ( y -- ) 0 swap set-cursor 49 spaces ;
: draw ( -- )
  0 0 set-cursor 0 7 set-colors
  ." LANTERN KEEP" 20 spaces cr
  map-height 0 ?do i draw-row loop
  draw-items
  0 13 set-cursor 0 7 set-colors
  ." Health: " health @ . ." Oil: " oil @ . 5 spaces cr
  ." Relics: " relics-found @ . ." / " required-relics . ."   Best: " best-score @ . 5 spaces cr
  ." W/A/S/D move | bump to attack | Q quit" cr
  21 16 ?do i clear-line loop
  0 16 set-cursor draw-message
  0 18 set-cursor
  game-state @ escaped = if ." ESCAPED! Score: " score . then
  game-state @ defeated = if ." DEFEATED. Relics: " relics-found @ . then
  game-state @ lantern-out = if ." YOUR LANTERN WENT OUT. Relics: " relics-found @ . then
  game-state @ 0 > game-state @ quit < and if
    cr ." R restart | Q quit"
  then
  0 21 set-cursor ." Find 3 * relics, escape at >. g/G guards."
  0 22 set-cursor
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
  screen-size 24 < swap 50 < or 0= screen-ok !
  screen-ok @ if
    clrscr
    begin draw key session-key game-state @ quit = until
    0 7 set-colors clrscr
    ." Back at Forth. play starts a new run after quit." cr
  else ." Lantern Keep needs a 50 by 24 text display." cr then
;
new-game
." Lantern Keep loaded. Type play to begin." cr

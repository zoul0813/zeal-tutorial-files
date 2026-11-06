\ Episode 005 finished source. Load pinned forth.fs first.
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
: new-game ( -- )
  4 position ! 0 relics-found ! starting-oil oil !
;
new-game

\ Episode 003 finished source. Load pinned forth.fs first.
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

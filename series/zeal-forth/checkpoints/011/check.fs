\ Regression checks for this exact finished-source checkpoint.
variable failures
variable checks
: check ( flag -- ) checks @ 1 + checks ! 0= if failures @ 1 + failures ! then ;
: replay ( addr n -- ) 0 ?do dup i + c@ turn loop drop ;
: run-checks
  0 failures ! 0 checks !
  new-game
  -1 0 inside? 0= check 0 -1 inside? 0= check
  30 0 inside? 0= check 0 12 inside? 0= check
  29 11 inside? check
  120 turn oil @ starting-oil = check
  1 player-x ! 97 turn player-x @ 1 = check oil @ starting-oil = check
  new-game 3 player-x ! 3 player-y ! 119 turn
  relics-found @ 1 = check 3 2 tile-address c@ floor-char = check
  115 turn 119 turn relics-found @ 1 = check
  new-game 26 player-x ! 9 player-y !
  required-relics relics-found ! 1 oil ! 100 turn
  game-state @ escaped = check oil @ 1 = check
  97 turn player-x @ 27 = check
  new-game relics-found @ 0= check

  new-game 26 player-x ! 9 player-y ! 100 turn
  game-state @ playing = check message @ sealed-exit = check
  oil @ starting-oil 1 - = check
  new-game 26 player-x ! 9 player-y ! required-relics 1 + relics-found ! 100 turn
  game-state @ escaped = check
  new-game 1 oil ! 100 turn game-state @ lantern-out = check oil @ 0= check
  114 session-key player-x @ 2 = check oil @ starting-oil = check relics-found @ 0= check
  81 session-key game-state @ quit = check
  new-game s" wwwwwwwdwdddddddddddddddddddddddswaaaaaaassssaaaassddddsdddddddds" replay
  game-state @ escaped = check oil @ 36 = check
  relics-found @ required-relics = check
  clrscr ." CHECKPOINT 011" cr
  ." CHECKS: " checks @ . cr ." FAILURES: " failures @ . cr .s cr
;
run-checks

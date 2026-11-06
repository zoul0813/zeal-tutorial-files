\ Regression checks for this exact finished-source checkpoint.
variable failures
variable checks
: check ( flag -- ) checks @ 1 + checks ! 0= if failures @ 1 + failures ! then ;
: replay ( addr n -- ) 0 ?do dup i + c@ turn loop drop ;
: run-checks
  0 failures ! 0 checks !
  new-game 100 turn position @ 5 = check oil @ 19 = check
  120 turn oil @ 19 = check
  new-game s" aaaadddddddd" replay
  position @ 8 = check relics-found @ 1 = check
  game-state @ 1 = check oil @ 9 = check
  new-game game-state @ 0= check relics-found @ 0= check

  new-game 0 position ! 97 turn position @ 0= check oil @ 20 = check
  new-game 8 position ! 100 turn position @ 8 = check oil @ 20 = check
  new-game 68 turn position @ 5 = check oil @ 19 = check
  new-game 81 turn game-state @ 4 = check oil @ 20 = check
  new-game 1 oil ! 100 turn game-state @ 3 = check oil @ 0= check
  114 session-key position @ 4 = check oil @ 20 = check relics-found @ 0= check
  113 session-key game-state @ 4 = check
  clrscr ." CHECKPOINT 008" cr
  ." CHECKS: " checks @ . cr ." FAILURES: " failures @ . cr .s cr
;
run-checks

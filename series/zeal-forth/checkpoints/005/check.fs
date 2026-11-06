\ Regression checks for this exact finished-source checkpoint.
variable failures
variable checks
: check ( flag -- ) checks @ 1 + checks ! 0= if failures @ 1 + failures ! then ;
: run-checks
  0 failures ! 0 checks !
  new-game position @ 4 = check oil @ 20 = check relics-found @ 0= check

  7 position ! 3 oil ! 1 relics-found ! new-game new-game
  position @ 4 = check oil @ 20 = check relics-found @ 0= check

  clrscr ." CHECKPOINT 005" cr
  ." CHECKS: " checks @ . cr ." FAILURES: " failures @ . cr .s cr
;
run-checks

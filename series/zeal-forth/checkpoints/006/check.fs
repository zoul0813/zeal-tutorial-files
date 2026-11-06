\ Regression checks for this exact finished-source checkpoint.
variable failures
variable checks
: check ( flag -- ) checks @ 1 + checks ! 0= if failures @ 1 + failures ! then ;
: run-checks
  0 failures ! 0 checks !
  new-game exit-open? 0= check lantern-lit? check
  1 relics-found ! exit-open? check new-game relics-found @ 0= check

  2 relics-found ! exit-open? check
  0 oil ! lantern-lit? 0= check
  new-game 0 position ! pickup pickup relics-found @ 1 = check
  new-game 8 position ! check-exit game-state @ playing = check
  1 relics-found ! check-exit game-state @ escaped = check
  new-game 1 relics-found ! check-exit game-state @ playing = check

  clrscr ." CHECKPOINT 006" cr
  ." CHECKS: " checks @ . cr ." FAILURES: " failures @ . cr .s cr
;
run-checks

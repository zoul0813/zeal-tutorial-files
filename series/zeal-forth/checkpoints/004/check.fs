\ Regression checks for this exact finished-source checkpoint.
variable failures
variable checks
: check ( flag -- ) checks @ 1 + checks ! 0= if failures @ 1 + failures ! then ;
: run-checks
  0 failures ! 0 checks !
  3 17 total-score 317 = check
  9 relic-score 900 = check
  corridor-length 9 = check
  starting-oil 20 = check

  clrscr ." CHECKPOINT 004" cr
  ." CHECKS: " checks @ . cr ." FAILURES: " failures @ . cr .s cr
;
run-checks

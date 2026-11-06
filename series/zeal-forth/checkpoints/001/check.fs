\ Regression checks for this exact finished-source checkpoint.
variable failures
variable checks
: check ( flag -- ) checks @ 1 + checks ! 0= if failures @ 1 + failures ! then ;
: run-checks
  0 failures ! 0 checks !
  6 7 * 42 = check

  clrscr ." CHECKPOINT 001" cr
  ." CHECKS: " checks @ . cr ." FAILURES: " failures @ . cr .s cr
;
run-checks

\ Regression checks for this exact finished-source checkpoint.
variable failures
variable checks
: check ( flag -- ) checks @ 1 + checks ! 0= if failures @ 1 + failures ! then ;
: run-checks
  0 failures ! 0 checks !
  3 17 swap 100 * + 317 = check
  17 dup + 34 = check
  5 2 - 3 = check
  2 5 - -3 = check

  clrscr ." CHECKPOINT 002" cr
  ." CHECKS: " checks @ . cr ." FAILURES: " failures @ . cr .s cr
;
run-checks

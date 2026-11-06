\ Rendering-state checks. Load after the pinned library and a game at either oil budget.
variable failures
variable checks
: check ( flag -- ) checks @ 1 + checks ! 0= if failures @ 1 + failures ! then ;
variable saved-health
variable saved-oil
variable saved-seed
variable saved-x
variable saved-y
: test-view ( -- )
  new-game health @ saved-health ! oil @ saved-oil ! seed @ saved-seed !
  player-x @ saved-x ! player-y @ saved-y !
  draw
  health @ saved-health @ = check oil @ saved-oil @ = check
  seed @ saved-seed @ = check player-x @ saved-x @ = check
  player-y @ saved-y @ = check relics-found @ 0= check
  0 guard-x @ 10 = check 0 guard-y @ 2 = check
;

0 failures ! 0 checks ! test-view
clrscr ." DRAW CHECKS: " checks @ . cr
." DRAW FAILURES: " failures @ . cr .s cr

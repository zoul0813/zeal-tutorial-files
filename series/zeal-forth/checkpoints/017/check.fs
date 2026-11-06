\ Load after forth.fs and keep.fs in a fresh interpreter.
\ Test-only state setup is deliberate; game input never supplies raw addresses.
variable failures
variable checks
: check ( flag -- )
  checks @ 1 + checks !
  0= if failures @ 1 + failures ! then
;
: replay ( addr length -- ) 0 ?do dup i + c@ turn loop drop ;
: quiet-game ( -- ) new-game 0 guard-count ! ;
: test-bounds ( -- )
  -1 0 inside? 0= check 0 -1 inside? 0= check
  30 0 inside? 0= check 0 12 inside? 0= check
  0 0 inside? check 29 11 inside? check
  new-game 120 turn oil @ starting-oil = check
  seed @ 12345 = check player-x @ 2 = check
  1 player-x ! 97 turn player-x @ 1 = check
  oil @ starting-oil = check
  quiet-game 68 turn player-x @ 3 = check
  oil @ starting-oil 1 - = check
;
: test-pickups ( -- )
  quiet-game 3 player-x ! 3 player-y ! 119 turn
  relics-found @ 1 = check 3 2 tile-address c@ floor-char = check
  115 turn 119 turn relics-found @ 1 = check
  quiet-game 2 relics-found ! 3 player-x ! 3 player-y ! 119 turn
  relics-found @ 3 = check message @ last-relic = check
  new-game relics-found @ 0= check
  3 2 tile-address c@ relic-char = check
;
: test-exit ( -- )
  quiet-game 26 player-x ! 9 player-y ! 100 turn
  game-state @ playing = check message @ sealed-exit = check
  oil @ starting-oil 1 - = check
  new-game 26 player-x ! 9 player-y !
  3 relics-found ! 1 oil ! 100 turn
  game-state @ escaped = check oil @ 1 = check
  seed @ 12345 = check 0 guard-x @ 10 = check
  best-score @ 301 = check
  97 turn player-x @ 27 = check
  new-game best-score @ 301 = check seed @ 12345 = check
;
: test-combat ( -- )
  new-game 1 guard-count ! 3 0 guard-x ! 9 0 guard-y !
  4 0 guard-hp ! 100 turn
  player-x @ 2 = check 0 guard-hp @ 2 = check
  health @ starting-health 1 - = check
  message @ struck-guard = check guard-message @ guard-hit = check
  100 turn 0 guard-hp @ 0= check
  health @ starting-health 1 - = check
  new-game 2 guard-count !
  3 0 guard-x ! 9 0 guard-y ! 4 0 guard-hp !
  28 1 guard-x ! 1 1 guard-y ! east 1 guard-dir !
  1 health ! 100 turn
  game-state @ defeated = check health @ 0= check
  oil @ starting-oil = check seed @ 12345 = check
  quiet-game 1 oil ! 100 turn
  game-state @ lantern-out = check oil @ 0= check
;
: test-guards ( -- )
  new-game 0 active-guard !
  3 nx ! 2 ny ! guard-can-step? 0= check
  27 nx ! 9 ny ! guard-can-step? 0= check
  -1 nx ! 0 ny ! guard-can-step? 0= check
  30 nx ! 0 ny ! guard-can-step? 0= check
  22 nx ! 7 ny ! guard-can-step? 0= check
  2 nx ! 9 ny ! guard-can-step? 0= check
  4 nx ! 4 ny ! guard-can-step? check
  3 0 guard-x ! 10 0 guard-y ! guard-adjacent? 0= check
  3 0 guard-x ! 9 0 guard-y ! guard-adjacent? check
  0 guard-count ! guards-turn health @ starting-health = check
;
: test-route ( -- )
  new-game
  s" wwwwwwwdwdddddddddddddddddddddddswaaaaaaassssaaaassddddsdddddddds" replay
  game-state @ escaped = check relics-found @ 3 = check
  player-x @ 27 = check player-y @ 9 = check
  oil @ 11 = check health @ 12 = check score 311 = check
  seed @ 39858 = check
;
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
: test-session ( -- )
  new-game 3 game-state ! 1 oil ! 2 relics-found ! 42 seed !
  114 session-key
  oil @ starting-oil = check relics-found @ 0= check
  seed @ 12345 = check game-state @ playing = check
  81 session-key game-state @ quit = check
  100 turn oil @ starting-oil = check
  new-game 400 best-score !
  26 player-x ! 9 player-y ! 4 relics-found ! 1 oil ! 100 turn
  game-state @ escaped = check best-score @ 401 = check
  new-game 26 player-x ! 9 player-y ! 3 relics-found ! 1 oil ! 100 turn
  best-score @ 401 = check
;
variable saved-best
: run-checks ( -- )
  best-score @ saved-best !
  0 failures ! 0 checks ! 0 best-score !
  new-game 0 guard-hp @ 2 = check 2 guard-hp @ 4 = check
  2 kind-damage kind@ 2 = check 2 kind-glyph kind@ 71 = check
  test-bounds test-pickups test-exit test-combat test-guards test-route test-view test-session
  saved-best @ best-score !
  clrscr ." KEEP CHECKS: " checks @ . cr
  ." KEEP FAILURES: " failures @ . cr .s cr
  ." FREE BYTES: " free . cr
;
run-checks

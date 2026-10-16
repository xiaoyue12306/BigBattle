# BigBattle working agreement

- Target Tuanjie 1.9.3 / editor 2022.3.62t11, Windows x64. This is a native Unity project, not a browser game.
- Read README.md, Docs/DESIGN.md, and update.md before changing the project.
- Every completed update must append a dated entry to update.md: changes, validation, known limits, and next steps. Commit and sync to https://github.com/xiaoyue12306/BigBattle after validation. Do not claim sync without checking the remote commit.
- MVP scope: one keyboard OR Xbox controller controls one banana assassin; facing-based attacks without a mouse; training dummy and a short solo PvE arena battle.
- Future scope: up to FIVE local players (three Xbox controllers and two keyboard players), automatic split screen, cooperative and versus modes. Do not present future features as implemented.
- Keep device input, actors, combat, session rules, and cameras separate. Two keyboard layouts may share the Windows keyboard stream; distinguishing two physical keyboards requires a future Windows Raw Input backend.
- No equipment, progression, matchmaking, minions, towers, or networking unless explicitly requested.
- Preserve .meta files. Ignore Library, Temp, Logs, Artifacts, and Builds. Do not commit system fonts, third-party art without a license, or generated executable binaries.
- Use Tools > BigBattle to rebuild the scene and Windows player. Run the standalone smoke test after gameplay changes. Record physical controller testing separately from automated input tests.

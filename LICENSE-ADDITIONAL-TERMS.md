# Additional Terms and Game Distribution Exception

**Project:** KaizenEngine
**Copyright (C) 2026 Matheus A. Santos, Hardion Labs**
**Repository:** https://github.com/CeoMatheusDev/KaizenEngine

The source code of KaizenEngine (the "Engine") is licensed under the **GNU Affero General Public License, version 3 or later (AGPL-3.0-or-later)**, the full text of which is in the file `LICENSE.txt`.

The terms below are **Additional Permissions** granted under **Section 7** of the AGPL-3.0, and **additional notice obligations** arising from **Section 5(a)** of the AGPL-3.0. They are part of the license of the software and apply to everyone who receives the Engine. They may not be removed or altered by subsequent distributors.

---

## 1. Game Distribution Exception

**Why this exception exists:** Under the AGPL, a work that links against or imports the Engine may be considered a "combined work", which would force the entire game to be licensed under the AGPL (and, because of the network clause, would force the disclosure of the source code for online games). This exception **limits the copyleft to the Engine itself**, not to games created with it, allowing you to distribute your game under whatever terms you choose, including proprietary and commercial terms.

### The Grant (The Permission)

**Additional permission under Section 7 of the GNU AGPL version 3.**

As a special exception, the copyright holder of KaizenEngine grants permission to combine the Engine with your independent work (a "Game") and to distribute the resulting combined work under the terms of your choice, **including proprietary and commercial terms**, **provided that**:

1. You distribute the **Engine itself**, and any modifications you make to it, under the **AGPL-3.0-or-later**, together with these Additional Terms.
2. The Game uses the Engine through its **public interfaces** and does not copy, reimplement, or incorporate a substantial part of the Engine's own source code beyond what is required to use it.
3. You display the attribution required in **Section 2** below.

**Important:** This exception is **optional**. If you do not wish to display the attribution, you may instead license your Game under the terms of the AGPL-3.0. Attribution is required **only** when you benefit from this exception to distribute your Game under non-AGPL terms (such as proprietary/commercial terms).

---

## 2. Attribution Requirements (Credits)

To benefit from the Game Distribution Exception above, you must provide attribution in a clear and visible manner.

- **In the Engine:** You must preserve the original Engine copyright notices and credits in any redistribution of the Engine (modified or not). Keeping the original `NOTICE` or `CREDITS.md` file is a way to comply with this requirement.
- **In the Game (Final Product):** You must include a visible attribution that the Game was "Made with KaizenEngine". This attribution may be provided:
  - On the Game's splash screen; or
  - In the Game's credits screen; or
  - In a text file `LICENSE.md` or `CREDITS.md` distributed together with the Game.
- **For Modifications to the Engine:** If you modify Engine files, you must keep the original credits for those files and add your own name as a contributor of the modification. Files created by you from scratch (without pre-existing Engine code) are your authorship, but the work as a whole (the modified Engine) remains under the AGPL-3.0 with these Additional Terms.

---

## 3. Modification Notices in Files (Section 5(a) of the AGPL)

**This obligation arises directly from the AGPL-3.0, not from a project choice.** Section 5(a) of the AGPL requires that every modified file carry a prominent notice stating that the file has changed and the date of the change.

### What is required

Every Engine source file that is modified must contain, in its header, a notice with:

1. A statement that the file has been **modified**;
2. The **name** of the person or organization that made the modification;
3. The **date** of the modification (format `YYYY-MM-DD`).

### Recommended format

```cpp
// Modified by [Name], [Organization] on [YYYY-MM-DD].
```

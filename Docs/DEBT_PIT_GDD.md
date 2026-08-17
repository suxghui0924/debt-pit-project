# DEBT PIT — Game Design Document

## 1. Game Overview

**Title:** DEBT PIT  
**Genre:** First-person labor roguelite / economic progression / psychological horror  
**Platform:** Windows PC  
**Perspective:** First person  
**Session structure:** Five-minute in-game days  
**Core theme:** Debt, obedience, memory, manufactured freedom

DEBT PIT places the player inside a government-operated labor facility. The prisoner has been stripped of their memories and must earn enough labor value every day to avoid execution. Surplus earnings may be deposited into a massive Freedom Fund. The facility promises release when the fund is completed, and it never technically lies.

When the player finally pays the full amount, they discover that “release” means memory removal and reassignment to DAY 1. The apparent escape was always another part of the loop.

## 2. Design Pillars

### Pressure

Every day has a visible deadline. The player must constantly decide whether to secure today's mandatory payment or risk investing in tools, packs, upgrades, gambling, and long-term progression.

### Escalation

The economy begins with rewards worth only a few labor points. As levels, card packs, tools, and upgrades unlock, values grow into thousands, millions, and eventually tens of millions.

### Suspicion

The facility presents clean rules and predictable systems, but environmental clues and daily story events reveal that the player has completed this cycle before.

### Physical interaction

Important systems are tied to places in the cell: the prisoner terminal, delivery chute, shop, and workbench. The player moves between them instead of controlling everything through one abstract menu.

## 3. Player Objective

### Short-term objective

Earn and pay the daily labor fee before midnight.

### Mid-term objective

Buy card packs, open containers, sell valuable cards, improve negotiation, expand inventory capacity, and unlock better tools.

### Long-term objective

Deposit **100,000,000 labor value** into the Freedom Fund.

### Hidden narrative objective

Discover what the Freedom Fund actually does and recognize that the player is trapped in a repeating memory-erasure program.

## 4. Core Gameplay Loop

1. Begin a new day and receive a story event.
2. Check the remaining time and daily labor payment.
3. Use the computer to claim rewards, buy packs, purchase tools, gamble, or manage payments.
4. Collect deliveries from the delivery chute.
5. Open card packs and sealed containers at the workbench.
6. Complete pack-specific minigames and obtain cards or valuables.
7. Sell items directly or negotiate for a better price.
8. Upgrade inventory, luck, negotiation, tools, and progression bonuses.
9. Pay the mandatory daily labor fee.
10. Deposit surplus labor value into the Freedom Fund.
11. Reach midnight, view the next story event, and repeat.

## 5. Day Structure

Each day lasts approximately **five real-time minutes**.

The HUD displays:

- Current day
- Player level
- Labor value
- Daily payment amount and payment status
- Freedom Fund progress
- Time remaining until midnight

Story, tutorial, loading, pause, and blocking UI sequences pause the daily timer. The remaining time is saved and restored when loading a game.

If the player reaches midnight without paying, an execution sequence begins. The save becomes invalid, and the player chooses between starting over and returning to the title screen.

## 6. Economy and Progression

### Currency

The primary currency is **labor value**. It represents both money and the player's right to remain alive.

Labor value is used for:

- Daily survival payments
- Freedom Fund deposits
- Card packs
- Tools
- Night-market containers
- Upgrades
- Gambling bets
- Market rerolls

### Growth curve

The economy should grow aggressively:

| Stage | Typical reward | Player feeling |
|---|---:|---|
| Early | 1–100 | Every item matters |
| Developing | 100–10,000 | Builds begin to form |
| Advanced | 10,000–1,000,000 | Strong acceleration |
| Endgame | 1,000,000–10,000,000+ | Freedom appears reachable |

Daily payment growth must remain threatening without consuming all income. A target balance is roughly 25–40% of an average competent player's daily earnings.

## 7. Card System

The card shop contains **30 distinct card-pack tiers**. Each pack has its own:

- Name and visual identity
- Required player level
- Purchase price
- Rarity distribution
- Card value multiplier
- Experience reward
- Opening protocol or minigame

Higher-level packs should not merely reskin the same interaction. Opening methods can include:

- Timing rings
- Charge and release
- Sequence memory
- Code reconstruction
- Signal alignment
- Portable-computer hacking
- Pressure stabilization
- Rotating seal matching

Cards are primarily sold for labor value, but rare cards may also unlock lore, upgrades, tools, or alternative interactions.

## 8. Workbench and Tools

The workbench is used for:

- Opening card packs
- Unlocking sealed containers
- Drilling reinforced boxes
- Cutting specialized locks
- Performing pack-specific minigames

Example tools:

- Lockpick
- Portable drill
- Cooling spray
- Hydraulic cutter
- Mini laptop
- Signal decoder
- Voltage stabilizer
- Inspection light

Container rarity controls minigame difficulty. For example, rarer lockboxes have a narrower success zone and may require more expensive tools.

## 9. Computer Applications

The prisoner terminal uses a Windows 95-inspired desktop interface with draggable application windows.

Applications include:

- Prisoner overview
- Daily payment
- Freedom Fund
- Daily reward
- Card shop
- Tool shop
- Upgrade shop
- Delivery monitor
- Risk game
- End day
- Help and tutorials
- Facility radio

The terminal unlocks the cursor while open. Leaving its interaction trigger automatically closes the interface and restores first-person controls.

## 10. Shop and Night Market

### Selling

The shop displays inventory items and their base value. Players can:

- Sell an individual item
- Sell all eligible items
- Negotiate before selling

Negotiation allows up to five attempts. Each successful attempt increases the offer, while failures consume an attempt and may end without improvement. Upgrades improve success chance and profit margins.

### Night market

The night market sells randomized containers and suspicious goods. Its stock refreshes every minute. Players may force a reroll, but reroll costs increase based on both the current day and the number of rerolls that day.

Night-market stock, reroll count, seed, and refresh timer are saved.

## 11. Inventory and Delivery

The hotbar begins with four slots and can be upgraded to ten. Selection supports number keys and the mouse wheel.

Purchased and rewarded items enter the delivery chute before reaching the inventory. Players can collect items individually or use “Collect All.” If inventory capacity is insufficient, remaining items stay safely in the chute instead of being deleted.

## 12. Upgrade System

Permanent upgrades include:

- Inventory capacity
- Negotiation success chance
- Negotiation profit margin
- Card-pack luck
- Risk-game payout
- Tool-store discount
- Minigame success-window size
- Experience gain

Upgrade prices should rise exponentially and act as a competing investment against the Freedom Fund.

## 13. Risk Game

The computer includes a simple gambling application. The player selects a labor-value bet and risks losing it for a multiplied payout.

The system exists to create temptation under time pressure. It should never be the safest long-term strategy, but upgrades may make specialized gambling builds viable.

## 14. Story Structure

### Opening

The player learns that their name and memories were removed as punishment. They must pay a daily labor fee and are promised freedom after completing the Freedom Fund.

### Daily revelations

Daily events gradually reveal the truth:

- Unrequested ration cans contain hidden letters.
- Other terminals use the same anonymous identity.
- Old execution records contain the player's prisoner number.
- A recording contains the player's own voice.
- Exit cards already contain the player's fingerprints.
- System errors mention memory recovery and DAY 1 reassignment.

### Failure ending

If the daily payment is missed, the facility executes the prisoner. The facility did not lie: survival was conditional on payment.

### Freedom ending

Completing the Freedom Fund immediately starts the release sequence. The exit leads to a memory-removal chair. The government fulfills its promise by removing the current identity from the facility and installing a blank version back into DAY 1.

The final line returns to the opening:

> DAY 01. You have lost your name.

## 15. Save System

The save records:

- Day, labor value, debt, and Freedom Fund
- Daily payment and reward status
- Inventory and delivery queue
- Player level, experience, and upgrades
- Remaining time until midnight
- Night-market stock and refresh state
- Tutorial progress
- Radio URL, playback position, playback state, loop setting, and volume

A failed execution or completed ending invalidates the current run so the title screen cannot load a finished save.

## 16. Audio Direction

The game uses industrial ambience, mechanical hum, distant impacts, metal footsteps, computer sounds, and restrained horror music.

Audio groups:

- Master
- BGM
- SFX
- Computer radio

The radio is spatialized from the physical computer. Volume can reach 500%, with higher values increasing audible distance. Local MP3, OGG, WAV, and direct audio URLs are supported.

## 17. Visual Direction

The world should feel dirty, compressed, and surveillance-heavy:

- Pixelated or dithered rendering
- Rust, exposed pipes, metal cages, and harsh practical lighting
- Deep blacks with dirty red and amber highlights
- Minimal clean UI outside the deliberately outdated computer interface
- Strong cinematic bars and fades for story transitions

The visual language should avoid excessive decorative cards, gradients, glowing elements, and modern mobile-game styling.

## 18. Tutorial Plan

The tutorial is contextual rather than delivered all at once.

Required lessons:

1. Daily survival payment and midnight deadline
2. Computer interaction
3. Delivery collection
4. Workbench and pack opening
5. Inventory limitations
6. Selling and negotiation
7. Freedom Fund deposits
8. New minigames and required tools
9. Night-market refresh and rerolls

Tutorial progress is saved so completed lessons do not repeat unnecessarily.

## 19. Development Priorities

### Milestone 1 — Stable vertical slice

- Complete one polished day
- Validate saving and loading
- Finalize payment failure sequence
- Finalize one card pack, one container, shop selling, and one upgrade path

### Milestone 2 — Progression depth

- Balance 30 card packs
- Expand cards and container rewards
- Add complete tool progression
- Tune exponential economy

### Milestone 3 — Narrative campaign

- Complete daily story events
- Produce required story images
- Add ending-specific sound and visuals
- Polish tutorial pacing

### Milestone 4 — Release polish

- Performance and crash testing
- Controller and resolution testing
- Audio normalization
- Localization QA
- Save migration and corruption recovery

## 20. Success Criteria

The design succeeds when the player:

- Feels constant pressure from the clock without becoming helpless
- Experiences satisfying economic escalation
- Understands how physical stations connect to the economy
- Develops a preferred pack, tool, and selling strategy
- Suspects the ending before reaching it but still wants to complete the fund
- Feels that the final reveal was honestly foreshadowed by the facility's wording

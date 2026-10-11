This mod fixes the issue that causes your game to "freeze" in the way that you can't zoom in or out and buttons do not work, and it also retries sprite rendering when buildings or units stay invisible while still existing in the world.
These were the issues:
Map data failing to load
Buildings or certain things that relied on kingdom related values to function also caused that crash
Some building or unit sprites were returning null during render checks, so they stayed invisible until refreshed
Usually, this is more common when you use a building placer and press shift to spread buildings all over the place.

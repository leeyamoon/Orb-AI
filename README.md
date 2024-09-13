
# Orb-AI
Orb is a 2D platformer.

*Link to the game in itch.io (Windows & Mac):*
https://tavp.itch.io/orb

The project was developed in Unity 2021.3.24f1.



**Made by:** Rom Meltser, Itamar Levine, Leeyam Gabay & Ariel Rimon.


## Installation

To run the project you need to use some version of Unity 2021.3, we recommend using 2021.3.24f1.

To open the project you need to open the "Orb-AI-Pro" file with Unity Hub.

To load the Q-values we learned you need to run the *"Game-QLearn"* scene and to copy the file QDict that in:

```bash
Orb-AI/Q-dicts/Merged/
```

Inside the directory (In windows)

```bash
C:\Users\<PlaceYourUser>\AppData\LocalLow\Orb\Orb
```
    
## Instructions

There are two relevant scenes in the project, the *"Game-QLearn"* scene for Q-learning algorithms, and the *"Game-Heuristic"* for both A*.

You can view all the scripts through:
```bash
Orb-AI-Pro/Assets/Scripts/AI-Scripts
```

Although the final agents use the "Merged Reward Function" and the "BFS Heuristic" we still have the previous implementations (Smart\Explorer Reward & Naive A*). To use those functions you just need to uncomment them and to comment the Merged\BFS functions.
## Notes

Pay attention that every minor changes in the scenes hierarchy can damage the behavior of the agents. 


## Support

For support, questions or anything else you can contact with **Leeyam Gabay**.

Email: leeyam.gabay@mail.huji.ac.il

WhatsApp\phone: 054-4927472


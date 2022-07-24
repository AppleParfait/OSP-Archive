# OSP-Archive

![papers407s1_OSP_RepresentativeImage](https://user-images.githubusercontent.com/38095565/180640662-abafe0bd-ec50-44e4-b371-fb7c73e5e2a8.jpg)

## Dynamic Optimal Space Partitioning for Redirected Walking in Multi-user Environment
SIGGRAPH 2022 & ACM Trans. Graph. 41, 4, Article 90 (July 2022), https://doi.org/10.1145/3528223.3530113

In multi-user Redirected Walking (RDW), the space subdivision method divides a shared physical space into sub-spaces and allocates a sub-space to each user. While this approach has the advantage of precluding any collisions between users, the conventional space subdivision method suffers from frequent boundary resets due to the reduction of available space per user. To address this challenge, in this study, we propose a space subdivision method called Optimal Space Partitioning (OSP) that dynamically divides the shared physical space in real-time. By exploiting spatial information of the physical and virtual environment, OSP predicts the movement of users and divides the shared physical space into optimal sub-spaces separated with shutters. Our OSP framework is trained using deep reinforcement learning to allocate optimal sub-space to each user and provide optimal steering. Our experiments demonstrate that OSP provides higher sense of immersion to users by minimizing the total number of reset counts, while preserving the advantage of the existing space subdivision strategy: ensuring better safety to users by completely eliminating the possibility of any collisions between users beforehand.

https://user-images.githubusercontent.com/38095565/180641556-947ff215-61b1-4b9e-8522-0ff7f38db13d.mp4

https://user-images.githubusercontent.com/38095565/180641563-f2541198-031b-4b1b-b77c-2b718b552de4.mp4

### Supplementary Materials
- 5 minutes video: https://www.youtube.com/watch?v=Vq7TRMC1cB4
- Supplementray doc: http://cga.yonsei.ac.kr/uploads/OSP_SuppleDoc.pdf

### Setting
- Unity3D Engine (2020.3.0f1) https://unity3d.com/kr/get-unity/download
- Unity ML-Agents Toolkit (ver. 0.25.0) https://github.com/Unity-Technologies/ml-agents
- Visual Studio 2019 https://visualstudio.microsoft.com/ko/vs/older-downloads/

### Contact
- author's e-mail: ludens0508@yonsei.ac.kr
- lab. homepage: http://cga.yonsei.ac.kr

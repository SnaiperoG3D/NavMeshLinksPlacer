# NavMeshLinksPlacer

This script will automaticle create NavMesh Links beetween square placed **Mesh Terrains**.

# What is it for?

Lets say you have multiple terrains scene, 64 terrains **converted to meshes** and you need to bake NavMesh Surfaces for each one.
![Безымянный](https://user-images.githubusercontent.com/88710464/128842736-2860b698-0321-4c21-a6d8-0588bf686e9d.png) 

First of all, you will have to manually place NavMesh Surface on each **Mesh Terrain**, then after bake you will get a problem on each NavMesh edge. They all disconnected.
![Безымянный](https://user-images.githubusercontent.com/88710464/128844044-89231972-86e3-41c9-806e-375282961f51.png)

And here we come. This script will find all **Mesh Terrains**, sort them and place NavMesh Links on each edge. Also it can select all NavMesh Surfaces on **Mesh Terrains** to help you to make multiple settings change or bake NavMesh Surfaces.

# Limitations

This script was create special for **Gaia** exports and Unity NavMesh Components.
What does it means? 
1) You has to use new NavMesh Components system in your project.
2) Script was written for Gaia hierarchy export, if it will not compare with yours just change it for your needs.  
3) Script was written for **Mesh Terrains**. It means your terrains was exported to meshes with **Mesh Filter** component on each one.

P.S. Ive exposed some parameters to inspector to make it easier.  
P.S.S. In my case i export every terrain with LODs, so it might not work if you not, see **What we have here**, **Debug Mode** and **How it works** parts.  
Gaia: https://assetstore.unity.com/packages/tools/terrain/gaia-pro-terrain-scene-generator-155852  
NavMesh Components: https://github.com/Unity-Technologies/NavMeshComponents  
**Tested only with**  
**Unity 2019.4.26f1**  
**Gaia Pro (2.2.4-c5)**  
**NavMesh Components 2019.3 branch**

# How to use?

1) Make sure you have NavMesh Components in your project;
2) Place script at **Assets/Editor** folder. Now you can open script window from windows tab: **Selection -> NavMeshLinksPlacer**;
3) (if you stream terrains with levels) Make sure that **ALL** your terrain scenes are loaded ;
4) Change parameters and press **Make all GOOD**.

**What we have here?**  
![Безымянный](https://user-images.githubusercontent.com/88710464/128849205-f7af7996-1fec-4953-ad4a-64822a83ba88.png)
- Select NavMesh Surfaces - Will select all founded Mesh Terrains and if there is no NavMesh Surface, it will add it.
- meshWidth - Width of each Mesh Terrain. If you exported terrain to mesh, it was **Terrain Width** or **Terrain Length** in **Settings** your terrain
- navMeshLinkThickness - Thickness of each NavMesh Link. Basicaly its a distance between **Start** and **End Point** of **NavMesh Link**
- nevMeshLinkWidth - Width of each NavMesh Link
- Make all GOOD - Place all NavMesh Links between **Mesh Terrains**
- Clear All Links - Remove all generated NavMesh Links
- showDebugInfo - Opens hiden variables to debug script or change something to your needs

**Debug Mode**  
![image](https://user-images.githubusercontent.com/88710464/128850704-2a70a3e4-2767-425b-a1b9-c49128749847.png)
- nameStarts - How does each **Mesh Terrain** name begin. Script works by names.
- totalFoundMeshes - Total count of founded **Mesh Terrains**
- totalNavMeshLinks - Total count of placed NavMesh Links
- Find MeshTerrain - Find **Mesh Terrains** by name begin.
- Sort MeshTerrains - Sort all founded **Mesh Terrains** by **positions**. After that matrix with values will appear.
- Create NavMesh Links - Add NavMesh Links on every unique edge **only** between **Mesh Terrains**
- (Matrix) - Sorted array of **Mesh Terrains** by **positions**. Negative down left corner, positive up right corner. 

# How it works?

1) Script will find all GameObject with specific name. If you not using gaia, change this name in debug mode.
2) Script will sort all founded objects by posiitons, from negative: down left to positive: up right.
3) Script will create NavMesh Links on each edge center between meshes:
  - Script will find edge center (x, z) by meshWidth variable;
  - Script will find height (y) by closest vertex of \<MeshFilter>().mesh to **previosly** founded edge center
  - Script will create new GamesObject to store NavMesh Links for each **Mesh Terrain**
  - Script will add NavMesh Links on this GameObject with founded position.
  
# Cheers!

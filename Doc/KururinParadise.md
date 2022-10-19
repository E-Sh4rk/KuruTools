# Level editor for Kururin Paradise

To edit the levels of Kururin Paradise, you must place the *Kururin Paradise (Japan)* ROM into the level editor directory, with the name `input.gba`.

## Layers

Contrary to *Kuru Kuru Kururin*, a level can have up to 4 layers (but some layers can be unused):

- **Walls (layer 1)**: describe the walls, the 'safe zones' and the physical elements (starting/healing/ending zones, moving objects, etc)
- **Ground (layer 2)**: describe the ground
- **Ground 2 (layer 3)**: describe the ground and/or the background
- **Background (layer 4)**: describe the background

*NOTE:* The *Wall* and *Ground 2* layers are always used. 

## The grid editor

The grid editor is the same as for the [Kuru Kuru Kururin editor](./KuruKuruKururin.md).

The only difference is that the data for special objects is not stored in the top rows of the grid (it is stored in a separate location of the ROM).

## Special Objects

The *Special Objects* menu allows you to change to configure the moving objects of the map. You must edit the wall layer if you want to access it.

![objects](./kurupara_objects.png)

1. The red zone contains the parameters for the moving objects in the map. You can edit it manually, but you should refer to the green zone if you want to add a new moving object configuration or if you want to know what the parameters refer to.
 
2. The green zone allows you to add a new special object configuration. All the fields are numbers. The ID field should be unique (two different entries should not have the same ID) and all the IDs should be consecutive (you can leave this field empty to use the next available ID).

When you are done with the configuration, you can click on *Quit*.

If you want to insert a moving object at a specific location on the map, just put the special tile L at this location, followed by the ID of the configuration to use (for instance, with the configuration of the screenshot above, L2 would be a piston).

If you want to insert a key, you can directly insert a tile K on the map followed by the ID of the key (KeyID). You shouldn't use numbers greater than 15 for KeyID.

*NOTE:* The field KeyID is independant from the field ID (the first identifies a key and is global to all levels, the latter identifies a moving object and is local to a level).

If you want to insert a roller catcher, you can directly insert a tile C on the map followed by a number corresponding to its direction.

using UnityEngine;

public static class Structures
{
    public struct Structure
    {
        public (string layer, int reps)[] Blocks;
        public int Height;

        public void Spawn(World world, Vector3Int pos, Block.Id blockId)
        {
            pos.y += Height;
            int x = 0, y = 0, z = 0;
            
            for (int layerI = 0; layerI < Blocks.Length; layerI++)
            {
                (string layer, int reps) = Blocks[layerI];

                for (int r = 0; r < reps; r++)
                {
                    for (int i = 0; i < layer.Length; i++)
                    {
                        char c = layer[i];
                        
                        switch (c)
                        {
                            case '#':
                                world.SetBlock(blockId, pos.x + x, pos.y + y, pos.z + z);
                                x++;
                                break;
                            case ' ':
                                x++;
                                break;
                            case '\n':
                                y--;
                                x = 0;
                                break;
                        }
                    }

                    x = 0;
                    y = 0;
                    z++;
                }
            }
        }
    }
    
    public static readonly Structure Rook = new()
    {
        Blocks = new[]
        {
            (@"
  # #  
  ###
  ###






  ###
  ###", 1),
            (@"
       
 #   #
 #   #
  ###
  ###
  ###
  ###
  ###
  ###
 #####
 #####", 1),
            (@"
#     # 
#     #
#     #
 #####
 #####
 #####
 #####
 #####
 #####
#######
#######", 1),
            (@"
        
#     #
#     #
 #####
 #####
 #####
 #####
 #####
 #####
#######
#######", 1),
            (@"
#     # 
#     #
#     #
 #####
 #####
 #####
 #####
 #####
 #####
#######
#######", 1),
            (@"
       
 #   #
 #   #
  ###
  ###
  ###
  ###
  ###
  ###
 #####
 #####", 1),
            (@"
  # #  
  ###
  ###






  ###
  ###", 1)
        },
        Height = 11
    };
    
    public static readonly Structure Pawn = new()
    {
        Blocks = new[]
        {
            (@"
 
 








  ###
  ###", 1),
            (@"

  ###     
  ###
  ###
     
     
     
  ###
  ###
  ###
 #####
 #####", 1),
            (@"
  ###
 #####     
 #####
 #####
   # 
     
     
 #####
 #####
 #####
#######
#######", 1),
            (@"
  ###
 #####     
 #####
 #####
  ### 
     
     
 #####
 #####
 #####
#######
#######", 1),
            (@"
  ###
 #####     
 #####
 #####
   # 
     
     
 #####
 #####
 #####
#######
#######", 1),
            (@"

  ###     
  ###
  ###
     
     
     
  ###
  ###
  ###
 #####
 #####", 1),
            (@"
 
 








  ###
  ###", 1)
        },
        Height = 12
    };
    
    public static readonly Structure[] StructureArray = { Rook, Pawn };
}

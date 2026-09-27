using System;
using System.Collections.Generic;

public sealed class DungeonSpec {
 public int Index, AllowedItems;public string Name;public bool SmallKeys;
 public DungeonSpec(int index,string name,int items,bool keys){Index=index;Name=name;AllowedItems=items;SmallKeys=keys;}
 // SaveContext.inventory: dungeonItems at +0xA8; signed dungeonKeys at +0xBC.
 // Flags: boss key=1, compass=2, map=4. Indices from scene_table.h and Map_Init.
 public static DungeonSpec[] All={
  new DungeonSpec(0,"Árbol Deku",6,false),new DungeonSpec(1,"Caverna Dodongo",6,false),new DungeonSpec(2,"Jabu-Jabu",6,false),
  new DungeonSpec(3,"Templo del Bosque",7,true),new DungeonSpec(4,"Templo del Fuego",7,true),new DungeonSpec(5,"Templo del Agua",7,true),
  new DungeonSpec(6,"Templo del Espíritu",7,true),new DungeonSpec(7,"Templo de las Sombras",7,true),new DungeonSpec(8,"Fondo del Pozo",6,true),
  new DungeonSpec(9,"Caverna de Hielo",6,false),new DungeonSpec(10,"Torre de Ganon",1,false),
  new DungeonSpec(11,"Campo de entrenamiento Gerudo",0,true),new DungeonSpec(12,"Escondite de las ladronas Gerudo",0,true),new DungeonSpec(13,"Interior del castillo de Ganon",0,true)
 };
 public static DungeonSpec Find(int index){return Array.Find(All,d=>d.Index==index);}
 public static string KeyText(byte value){return value==255?"sin recoger":value>127?"valor especial "+value:value.ToString();}
 public static void Apply(byte[] b,int s,EditOptions options){
  foreach(var p in options.DungeonItems){var d=Find(p.Key);if(d==null||p.Value<=0||(p.Value&~d.AllowedItems)!=0)throw new Exception("Objeto no válido para esa mazmorra.");b[s+0xa8+p.Key]|=(byte)p.Value;}
  foreach(var p in options.DungeonKeys){var d=Find(p.Key);if(d==null||!d.SmallKeys||p.Value<0||p.Value>99)throw new Exception("Llaves no válidas para esa mazmorra (0–99).");b[s+0xbc+p.Key]=(byte)p.Value;}
 }
 public static void Describe(MemoryFile m,EditOptions options,List<string> lines){
  foreach(var p in options.DungeonItems){var names=new List<string>();if((p.Value&4)!=0)names.Add("mapa");if((p.Value&2)!=0)names.Add("brújula");if((p.Value&1)!=0)names.Add("llave de jefe");lines.Add(Find(p.Key).Name+": añadir "+String.Join(", ",names.ToArray()));}
  foreach(var p in options.DungeonKeys)lines.Add(Find(p.Key).Name+" · llaves pequeñas: "+KeyText(m.Bytes[m.Save+0xbc+p.Key])+" → "+p.Value);
 }
}

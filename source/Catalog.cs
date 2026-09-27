using System;
using System.Collections.Generic;

// SaveContext layouts checked against zeldaret/oot include/save.h, item.h
// and src/code/z_inventory.c. These are file edits, not runtime cheat hooks.
public sealed class UpgradeSpec {
 public string Key, Name; public int Shift, Mask, Slot; public string[] Levels; public int[] Capacities;
 public UpgradeSpec(string key,string name,int shift,int mask,int slot,string[] levels,int[] caps){Key=key;Name=name;Shift=shift;Mask=mask;Slot=slot;Levels=levels;Capacities=caps;}
 public static UpgradeSpec[] All={
  new UpgradeSpec("bombs","Bolsa de bombas",3,7,2,new[]{"Sin bolsa","20 bombas","30 bombas","40 bombas"},new[]{0,20,30,40}),
  new UpgradeSpec("wallet","Cartera",12,3,-1,new[]{"99 rupias","200 rupias","500 rupias","500 rupias (valor especial)"},new[]{99,200,500,500}),
  new UpgradeSpec("arrows","Carcaj",0,7,3,new[]{"Sin carcaj","30 flechas","40 flechas","50 flechas"},new[]{0,30,40,50}),
  new UpgradeSpec("seeds","Bolsa de semillas",14,7,6,new[]{"Sin bolsa","30 semillas","40 semillas","50 semillas"},new[]{0,30,40,50}),
  new UpgradeSpec("sticks","Capacidad de palos",17,7,0,new[]{"Sin mejora","10 palos","20 palos","30 palos"},new[]{0,10,20,30}),
  new UpgradeSpec("nuts","Capacidad de nueces",20,7,1,new[]{"Sin mejora","20 nueces","30 nueces","40 nueces"},new[]{0,20,30,40}),
  new UpgradeSpec("strength","Fuerza",6,7,-1,new[]{"Sin mejora","Brazalete Goron","Guantes de plata","Guantes de oro"},null),
  new UpgradeSpec("scale","Escama de buceo",9,7,-1,new[]{"Sin mejora","Escama de plata","Escama de oro"},null)
 };
 public static UpgradeSpec Find(string key){return Array.Find(All,x=>x.Key==key);}
 public int MaxLevel {get{return Key=="wallet"?2:Levels.Length-1;}}
 public int Read(byte[] bytes,int save){return (int)((MemoryFile.U32(bytes,save+0xa0)>>Shift)&(uint)Mask);}
 public string Describe(int n){return n<Levels.Length?Levels[n]:"Valor no reconocido ("+n+")";}
}
public sealed class ItemSpec {
 public int Slot;public string Name; public int[] Values;public string[] Names;
 public ItemSpec(int slot,string name,int[] values,string[] names){Slot=slot;Name=name;Values=values;Names=names;}
 public string Describe(int value){if(value==255)return "No adquirido";int i=Array.IndexOf(Values,value);return i<0?"Contenido especial ("+value+")":Names[i];}
 public static ItemSpec[] All=Build();
 static ItemSpec[] Build(){var list=new List<ItemSpec>();
  string[] names={"Palos Deku","Nueces Deku","Bombas","Arco","Flechas de fuego","Fuego de Din","Tirachinas","Ocarina","Bombchus","Gancho","Flechas de hielo","Viento de Farore","Bumerán","Lente de la verdad","Judías mágicas","Martillo Megatón","Flechas de luz","Amor de Nayru"};
  int[] ids={0,1,2,3,4,5,6,8,9,11,12,13,14,15,16,17,18,19};
  for(int i=0;i<18;i++)list.Add(new ItemSpec(i,names[i],i==7?new[]{7,8}:i==9?new[]{10,11}:new[]{ids[i]},i==7?new[]{"Ocarina de las hadas","Ocarina del Tiempo"}:i==9?new[]{"Gancho corto","Gancho largo"}:new[]{names[i]}));
  for(int i=18;i<22;i++)list.Add(new ItemSpec(i,"Botella "+(i-17),new[]{20,21,22,23,24,25,26,28,29,30,31,32},new[]{"Vacía","Poción roja","Poción verde","Poción azul","Hada","Pez","Leche","Fuego azul","Bichos","Gran Poe","Media leche","Poe"}));
  return list.ToArray();
 }
 public static ItemSpec Find(int slot){return Array.Find(All,x=>x.Slot==slot);}
}
public sealed class EquipmentSpec {
 public int Bit;public string Name;public EquipmentSpec(int bit,string name){Bit=bit;Name=name;}
 public static EquipmentSpec[] All={new EquipmentSpec(0,"Espada Kokiri"),new EquipmentSpec(1,"Espada Maestra"),new EquipmentSpec(2,"Espada Biggoron"),new EquipmentSpec(4,"Escudo Deku"),new EquipmentSpec(5,"Escudo Hyliano"),new EquipmentSpec(6,"Escudo Espejo"),new EquipmentSpec(8,"Túnica Kokiri"),new EquipmentSpec(9,"Túnica Goron"),new EquipmentSpec(10,"Túnica Zora"),new EquipmentSpec(12,"Botas Kokiri"),new EquipmentSpec(13,"Botas de hierro"),new EquipmentSpec(14,"Botas flotantes")};
 public bool Owned(byte[] b,int s){return (((b[s+0x9c]<<8)|b[s+0x9d])&(1<<Bit))!=0&&(Bit!=2||b[s+0x3e]!=0);}
}
public sealed class EditOptions {
 public bool AllSkulltulas,AllHeartPieces;
 public HashSet<string> Modules=new HashSet<string>();
 public Dictionary<string,int> Upgrades=new Dictionary<string,int>();
 public Dictionary<int,int> Items=new Dictionary<int,int>();
 public HashSet<int> Equipment=new HashSet<int>();
 public Dictionary<int,int> DungeonItems=new Dictionary<int,int>();
 public Dictionary<int,int> DungeonKeys=new Dictionary<int,int>();
 public bool Empty {get{return Modules.Count==0&&!AllSkulltulas&&!AllHeartPieces&&Upgrades.Count==0&&Items.Count==0&&Equipment.Count==0&&DungeonItems.Count==0&&DungeonKeys.Count==0;}}
}
public static class Catalog {
 static void Put16(byte[] b,int p,int value){b[p]=(byte)(value>>8);b[p+1]=(byte)value;}
 static void SetUpgrade(byte[] b,int s,UpgradeSpec spec,int value){uint word=MemoryFile.U32(b,s+0xa0);word=(word&~((uint)spec.Mask<<spec.Shift))|((uint)value<<spec.Shift);MemoryFile.W32(b,s+0xa0,word);}
 static void SetItem(byte[] b,int s,int slot,int value){
  int old=b[s+0x74+slot];if(old==value)return;
  // Synchronize item ids for that slot in current, child and adult C-button sets.
  // Icon textures in the captured frame are not rebuilt; re-equip in the game.
  if(old!=255)foreach(int equips in new[]{0x40,0x4a,0x68})for(int i=0;i<3;i++)if(b[s+equips+4+i]==slot&&b[s+equips+1+i]==old)b[s+equips+1+i]=(byte)value;
  b[s+0x74+slot]=(byte)value;
 }
 public static void Apply(byte[] b,int s,EditOptions options){
  Completion.Apply(b,s,options);
  DungeonSpec.Apply(b,s,options);
  Collections.Apply(b,s,options);
  foreach(var pair in options.Upgrades){var spec=UpgradeSpec.Find(pair.Key);if(spec==null||pair.Value<0||pair.Value>spec.MaxLevel)throw new Exception("Mejora no válida.");int current=spec.Read(b,s);if(current>spec.MaxLevel||pair.Value<current)throw new Exception("Solo se permiten mejoras, sin rebajar capacidades.");SetUpgrade(b,s,spec,pair.Value);
   if(spec.Slot>=0&&pair.Value>0){int old=b[s+0x74+spec.Slot];if(old!=255&&old!=spec.Slot)throw new Exception("Objeto no reconocido para esa mejora.");SetItem(b,s,spec.Slot,spec.Slot);}
  }
  foreach(var pair in options.Items){var spec=ItemSpec.Find(pair.Key);if(spec==null||Array.IndexOf(spec.Values,pair.Value)<0)throw new Exception("Objeto no válido para esa ranura.");int old=b[s+0x74+pair.Key];
   if(old!=255&&Array.IndexOf(spec.Values,old)<0)throw new Exception("Se conserva el contenido especial de esa ranura.");
   if(pair.Key<18&&old!=255&&Array.IndexOf(spec.Values,pair.Value)<Array.IndexOf(spec.Values,old))throw new Exception("No se rebajan objetos ya mejorados.");
   SetItem(b,s,pair.Key,pair.Value);
   foreach(var upgrade in UpgradeSpec.All)if(upgrade.Slot==pair.Key&&upgrade.Read(b,s)==0)SetUpgrade(b,s,upgrade,1);
  }
  foreach(int bit in options.Equipment){var spec=Array.Find(EquipmentSpec.All,x=>x.Bit==bit);if(spec==null)throw new Exception("Equipo no válido.");int owned=(b[s+0x9c]<<8)|b[s+0x9d];owned|=1<<bit;
   if(bit==2){owned&=~8;b[s+0x3e]=1;Put16(b,s+0x36,8);}Put16(b,s+0x9c,owned);
  }
 }
 public static int Capacity(byte[] b,int s,int offset){
  if(offset==0xd0)return 100;if(offset==0x2e)return 320;if(offset==0x30)return (b[s+46]<<8)|b[s+47];
  if(offset==0x33)return b[s+0x3a]==0?0:b[s+0x3c]!=0?96:48;
  if(offset==0x34){var w=UpgradeSpec.Find("wallet");return w.Capacities[w.Read(b,s)];}
  int slot=offset-0x8c;if(slot<0||slot>=16||b[s+0x74+slot]==255)return 0;
  if(slot==8)return 50;if(slot==14)return 10;
  foreach(var u in UpgradeSpec.All)if(u.Slot==slot&&u.Capacities!=null){int level=u.Read(b,s);return level<u.Capacities.Length?u.Capacities[level]:0;}return 0;
 }
 public static List<string> Describe(MemoryFile m,EditOptions options){var lines=new List<string>();foreach(var p in options.Upgrades){var u=UpgradeSpec.Find(p.Key);lines.Add(u.Name+": "+u.Describe(u.Read(m.Bytes,m.Save))+" → "+u.Describe(p.Value)+(u.Slot>=0?" (incluye el objeto)":""));}foreach(var p in options.Items){var i=ItemSpec.Find(p.Key);lines.Add(i.Name+": "+i.Describe(m.Bytes[m.Save+0x74+p.Key])+" → "+i.Describe(p.Value));}foreach(int bit in options.Equipment)lines.Add("Añadir "+Array.Find(EquipmentSpec.All,x=>x.Bit==bit).Name);DungeonSpec.Describe(m,options,lines);Collections.Describe(m,options,lines);Completion.Describe(m,options,lines);return lines;}
}

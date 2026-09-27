using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public partial class Editor {
 Dictionary<int,CheckBox[]> dungeonFlags=new Dictionary<int,CheckBox[]>();
 Dictionary<int,CheckBox> keyChange=new Dictionary<int,CheckBox>();
 Dictionary<int,NumericUpDown> keyValues=new Dictionary<int,NumericUpDown>();
 Dictionary<int,Label> keyOriginal=new Dictionary<int,Label>();
 Button mapsAll,bossAll;
 void BuildDungeonPage(){
  var page=Page("Mazmorras · experimental","Añade mapas, brújulas y llaves de jefe. Para las pequeñas, marca Cambiar y elige la cantidad (0–99).\nNo derrota jefes, abre puertas ni completa misiones. 99 es el tope del editor, no el total de cada mazmorra.");
  var grid=new TableLayoutPanel{Dock=DockStyle.Top,AutoSize=true,ColumnCount=5,Padding=new Padding(3)};
  grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,35));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,9));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,10));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,12));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,34));
  string[] heads={"Mazmorra","Mapa","Brújula","Llave jefe","Llaves pequeñas"};for(int i=0;i<5;i++)grid.Controls.Add(LabelText(heads[i]),i,0);int row=1;
  foreach(var d in DungeonSpec.All){grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));grid.Controls.Add(new Label{Text=d.Name,AutoSize=true,MaximumSize=new Size(295,0),Anchor=AnchorStyles.Left,Margin=new Padding(5)},0,row);var flags=new CheckBox[3];int[] masks={4,2,1};
   for(int i=0;i<3;i++){if((d.AllowedItems&masks[i])!=0){var c=new CheckBox{AutoSize=true,Enabled=false,Anchor=AnchorStyles.None};c.CheckedChanged+=(s,e)=>Changed();flags[i]=c;grid.Controls.Add(c,i+1,row);}else grid.Controls.Add(LabelText("—"),i+1,row);}dungeonFlags.Add(d.Index,flags);
   if(d.SmallKeys){int index=d.Index;var panel=new FlowLayoutPanel{AutoSize=true,WrapContents=false,Dock=DockStyle.Fill,Margin=new Padding(0)};var check=new CheckBox{Text="Cambiar",AutoSize=true,Enabled=false,Margin=new Padding(2,8,2,2)};var value=new NumericUpDown{Minimum=0,Maximum=99,Width=55,Enabled=false,Margin=new Padding(2,4,2,2)};var current=new Label{AutoSize=true,Margin=new Padding(3,8,0,2),ForeColor=Color.DimGray};check.CheckedChanged+=(s,e)=>{value.Enabled=check.Checked&&memory!=null;Changed();};value.ValueChanged+=(s,e)=>Changed();keyChange.Add(index,check);keyValues.Add(index,value);keyOriginal.Add(index,current);panel.Controls.AddRange(new Control[]{check,value,current});grid.Controls.Add(panel,4,row);}else grid.Controls.Add(LabelText("—"),4,row);row++;
  }
  var buttons=new FlowLayoutPanel{AutoSize=true,Dock=DockStyle.Fill};mapsAll=ButtonText("Añadir mapas y brújulas",235);mapsAll.Enabled=false;mapsAll.Click+=(s,e)=>SetDungeonFlags(false);bossAll=ButtonText("Añadir llaves de jefe",215);bossAll.Enabled=false;bossAll.Click+=(s,e)=>SetDungeonFlags(true);buttons.Controls.AddRange(new Control[]{mapsAll,bossAll});grid.Controls.Add(buttons,0,row);grid.SetColumnSpan(buttons,5);Body(page).Controls.Add(grid);
 }
 void PopulateDungeons(){foreach(var d in DungeonSpec.All){int stored=memory.Read(0xa8+d.Index,1);int[] masks={4,2,1};var flags=dungeonFlags[d.Index];for(int i=0;i<3;i++)if(flags[i]!=null){bool owned=(stored&masks[i])!=0;flags[i].Checked=owned;flags[i].Enabled=!owned;}
   if(d.SmallKeys){byte raw=memory.Bytes[memory.Save+0xbc+d.Index];keyChange[d.Index].Checked=false;keyChange[d.Index].Enabled=true;keyValues[d.Index].Value=raw<=99?raw:0;keyValues[d.Index].Enabled=false;keyOriginal[d.Index].Text="Actual: "+DungeonSpec.KeyText(raw);}}
  mapsAll.Enabled=bossAll.Enabled=true;
 }
 void CollectDungeons(EditOptions result){foreach(var d in DungeonSpec.All){int add=0;int[] masks={4,2,1};var flags=dungeonFlags[d.Index];for(int i=0;i<3;i++)if(flags[i]!=null&&flags[i].Enabled&&flags[i].Checked)add|=masks[i];if(add!=0)result.DungeonItems.Add(d.Index,add);if(d.SmallKeys&&keyChange[d.Index].Checked)result.DungeonKeys.Add(d.Index,(int)keyValues[d.Index].Value);}}
 void SetDungeonFlags(bool boss){loading=true;foreach(var flags in dungeonFlags.Values)for(int i=0;i<3;i++)if((boss?i==2:i<2)&&flags[i]!=null&&flags[i].Enabled)flags[i].Checked=true;loading=false;Changed();}
}

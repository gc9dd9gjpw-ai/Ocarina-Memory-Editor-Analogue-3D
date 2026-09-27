using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
public partial class Editor {
 Dictionary<string,CheckBox> moduleChecks=new Dictionary<string,CheckBox>();Button completeAll;
 void BuildCompletionPage(){
  var page=Page("Completar · v3.0.0","Selecciona módulos independientes. El botón general reúne el catálogo documentado y las 100 Skulltulas.\nExperimental: consulta el alcance y las limitaciones antes de guardar.");
  var panel=new FlowLayoutPanel{Dock=DockStyle.Top,AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false};
  completeAll=ButtonText("🏆 COMPLETAR PARTIDA 100 % REAL",490);completeAll.Enabled=false;completeAll.Click+=(s,e)=>{loading=true;foreach(var c in moduleChecks.Values)c.Checked=true;allSkulltulas.Checked=allHeartPieces.Checked=true;inputs[0xd0].Value=100;inputs[0xd0].Enabled=false;inputs[0x2e].Value=inputs[0x30].Value=20;loading=false;Changed();tabs.SelectedIndex=tabs.TabCount-1;};panel.Controls.Add(completeAll);
  for(int i=0;i<Completion.Keys.Length;i++){var c=new CheckBox{Text=Completion.Names[i],AutoSize=true,Enabled=false,Margin=new Padding(5)};c.CheckedChanged+=(s,e)=>Changed();moduleChecks.Add(Completion.Keys[i],c);panel.Controls.Add(c);}
  panel.Controls.Add(new Label{Text=Completion.Warning,AutoSize=true,MaximumSize=new Size(780,0),ForeColor=Color.Firebrick,Margin=new Padding(5,14,5,10)});Body(page).Controls.Add(panel);
 }
}

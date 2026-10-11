import React, {useEffect, useState} from "react";
import {Alert, Pressable, SafeAreaView, ScrollView, StatusBar, StyleSheet, Text, TextInput, View} from "react-native";
import AsyncStorage from "@react-native-async-storage/async-storage";
import {api, login, register} from "./src/api";
import {API_BASE_URL, SIGNALR_URL} from "./src/config";
import {HubConnectionBuilder, LogLevel} from "@microsoft/signalr";

const C={bg:"#0B0F14",surface:"#121821",card:"#171F2A",blue:"#3B82F6",text:"#F8FAFC",muted:"#94A3B8",border:"#263241",green:"#22C55E",red:"#EF4444",orange:"#F59E0B"};

type User={id:number,name:string,email:string,role:string};
type Service={id:number,name:string,description:string,averageDuration:number,queueId:number};
type Business={id:number,name:string,address:string,description:string,services:Service[]};
type QueueState={queueId:number,status:string,currentNumber:number,waiting:number,myTicket?:number,peopleAhead:number,estimatedMinutes:number};

export default function App(){
  const [token,setToken]=useState<string|null>(null);
  const [user,setUser]=useState<User|null>(null);
  const [auth,setAuth]=useState<"login"|"register">("login");
  const [screen,setScreen]=useState<"home"|"queue"|"staff">("home");
  const [businesses,setBusinesses]=useState<Business[]>([]);
  const [active,setActive]=useState<QueueState|null>(null);
  const [selectedService,setSelectedService]=useState<Service|null>(null);

  useEffect(()=>{(async()=>{const t=await AsyncStorage.getItem("token"); const u=await AsyncStorage.getItem("user"); if(t&&u){setToken(t);setUser(JSON.parse(u));}})()},[]);
  useEffect(()=>{if(token){api.defaults.headers.common.Authorization=`Bearer ${token}`; loadBusinesses();}},[token]);
  useEffect(()=>{if(token&&active){let connection=new HubConnectionBuilder().withUrl(SIGNALR_URL,{accessTokenFactory:()=>token}).configureLogging(LogLevel.Error).withAutomaticReconnect().build(); connection.on("QueueUpdated",(s:QueueState)=>{if(s.queueId===active.queueId) setActive(s)}); connection.start().then(()=>connection.invoke("JoinQueueRoom",String(active.queueId))).catch(()=>{}); return()=>{connection.stop()}}},[token,active?.queueId]);

  async function loadBusinesses(){const r=await api.get("/businesses");setBusinesses(r.data)}
  async function doAuth(email:string,password:string,name?:string){
    try{const r=name?await register(name,email,password):await login(email,password); setToken(r.data.token);setUser(r.data.user);await AsyncStorage.setItem("token",r.data.token);await AsyncStorage.setItem("user",JSON.stringify(r.data.user));}
    catch(e:any){Alert.alert("QueueLess",e?.response?.data?.message||"Request failed")}
  }
  async function join(s:Service){try{const r=await api.post(`/queues/${s.queueId}/join`); const q=await api.get(`/queues/${s.queueId}`);setSelectedService(s);setActive(q.data);setScreen("queue");}catch(e:any){Alert.alert("QueueLess",e?.response?.data?.message||"Could not join queue")}}
  async function leave(){if(!active)return; await api.delete(`/queues/${active.queueId}/leave`);setActive(null);setScreen("home");}
  async function next(){if(!active)return; await api.post(`/queues/${active.queueId}/next`); const q=await api.get(`/queues/${active.queueId}`);setActive(q.data)}
  async function logout(){await AsyncStorage.multiRemove(["token","user"]);setToken(null);setUser(null);setActive(null)}

  if(!token||!user) return <Auth onSubmit={doAuth} mode={auth} setMode={setAuth}/>;

  return <SafeAreaView style={s.root}><StatusBar barStyle="light-content"/>
    <View style={s.header}><View><Text style={s.brand}>QueueLess</Text><Text style={s.muted}>{screen==="staff"?"Staff console":`Hi, ${user.name}`}</Text></View><Pressable onPress={logout}><Text style={s.link}>Sign out</Text></Pressable></View>
    {screen==="queue"&&active ? <QueueScreen q={active} service={selectedService} onLeave={leave}/> :
     screen==="staff"&&user.role==="Staff" ? <Staff q={active} onNext={next}/> :
     <Home businesses={businesses} onJoin={join} onStaff={()=>setScreen("staff")} isStaff={user.role==="Staff"}/>}
  </SafeAreaView>
}

function Auth({onSubmit,mode,setMode}:{onSubmit:(e:string,p:string,n?:string)=>void,mode:"login"|"register",setMode:(x:"login"|"register")=>void}){
 const [name,setName]=useState("");const [email,setEmail]=useState("customer@queueless.local");const [pass,setPass]=useState("Password123!");
 return <SafeAreaView style={s.root}><View style={s.auth}><Text style={s.hero}>QueueLess</Text><Text style={s.subtitle}>Join the line without standing in it.</Text>
  {mode==="register"&&<TextInput style={s.input} placeholder="Name" placeholderTextColor={C.muted} value={name} onChangeText={setName}/>}
  <TextInput style={s.input} placeholder="Email" placeholderTextColor={C.muted} autoCapitalize="none" value={email} onChangeText={setEmail}/>
  <TextInput style={s.input} placeholder="Password" placeholderTextColor={C.muted} secureTextEntry value={pass} onChangeText={setPass}/>
  <Button title={mode==="login"?"Sign in":"Create account"} onPress={()=>onSubmit(email,pass,name)}/>
  <Pressable onPress={()=>setMode(mode==="login"?"register":"login")}><Text style={s.centerLink}>{mode==="login"?"Create an account":"I already have an account"}</Text></Pressable>
  <Text style={s.demo}>Demo customer: customer@queueless.local / Password123!{"\n"}Demo staff: staff@queueless.local / Password123!</Text>
 </View></SafeAreaView>
}

function Home({businesses,onJoin,onStaff,isStaff}:{businesses:Business[],onJoin:(x:Service)=>void,onStaff:()=>void,isStaff:boolean}){
 return <ScrollView contentContainerStyle={s.content}><Text style={s.title}>Find a service</Text><Text style={s.muted}>Choose a queue and see the wait before you leave.</Text>
 {isStaff&&<Button title="Open staff console" onPress={onStaff}/>}
 {businesses.map(b=><View key={b.id} style={s.business}><Text style={s.businessName}>{b.name}</Text><Text style={s.muted}>{b.address}</Text>{b.services.map(sv=><View key={sv.id} style={s.service}><View style={{flex:1}}><Text style={s.serviceName}>{sv.name}</Text><Text style={s.muted}>{sv.description}</Text><Text style={s.small}>~{sv.averageDuration} min/service</Text></View><Button small title="Join" onPress={()=>onJoin(sv)}/></View>)}</View>)}</ScrollView>
}

function QueueScreen({q,service,onLeave}:{q:QueueState,service:Service|null,onLeave:()=>void}){
 return <ScrollView contentContainerStyle={s.content}><Text style={s.title}>Your queue</Text><Text style={s.muted}>{service?.name||"Service"}</Text>
 <View style={s.ticket}><Text style={s.muted}>YOUR NUMBER</Text><Text style={s.ticketNo}>#{q.myTicket??"—"}</Text><Text style={s.ahead}>{q.peopleAhead} people ahead</Text><View style={s.divider}/><Row label="Current number" value={`#${q.currentNumber||"—"}`}/><Row label="Estimated wait" value={`${q.estimatedMinutes} min`}/><Row label="People waiting" value={String(q.waiting)}/><View style={s.progress}><View style={{width:`${Math.min(100,Math.max(5,(q.myTicket?((q.myTicket-q.currentNumber+1)/(q.waiting+1))*100:5)))}%`,height:"100%",backgroundColor:C.blue,borderRadius:8}}/></View></View>
 <Text style={s.notice}>You will see live queue updates here.</Text><Button title="Leave queue" danger onPress={onLeave}/></ScrollView>
}
function Staff({q,onNext}:{q:QueueState|null,onNext:()=>void}){return <ScrollView contentContainerStyle={s.content}><Text style={s.title}>Staff console</Text><View style={s.ticket}><Text style={s.muted}>CURRENT</Text><Text style={s.ticketNo}>#{q?.currentNumber||"—"}</Text><Text style={s.ahead}>{q?.waiting||0} waiting</Text></View><Button title="Call next customer" onPress={onNext}/></ScrollView>}
function Row({label,value}:{label:string,value:string}){return <View style={s.row}><Text style={s.muted}>{label}</Text><Text style={s.rowValue}>{value}</Text></View>}
function Button({title,onPress,small,danger}:{title:string,onPress:()=>void,small?:boolean,danger?:boolean}){return <Pressable onPress={onPress} style={[s.button,small&&s.smallButton,danger&&s.danger]}><Text style={s.buttonText}>{title}</Text></Pressable>}

const s=StyleSheet.create({
 root:{flex:1,backgroundColor:C.bg},header:{paddingHorizontal:20,paddingTop:18,paddingBottom:12,flexDirection:"row",justifyContent:"space-between",alignItems:"center",borderBottomWidth:1,borderBottomColor:C.border},brand:{fontSize:24,fontWeight:"800",color:C.text},content:{padding:20,gap:14},title:{fontSize:30,fontWeight:"800",color:C.text},hero:{fontSize:48,fontWeight:"900",color:C.text,textAlign:"center"},subtitle:{fontSize:16,color:C.muted,textAlign:"center",marginBottom:24},auth:{padding:24,justifyContent:"center",flex:1},input:{backgroundColor:C.surface,borderWidth:1,borderColor:C.border,borderRadius:12,padding:15,color:C.text,marginBottom:12},button:{backgroundColor:C.blue,borderRadius:12,paddingVertical:14,paddingHorizontal:18,alignItems:"center",marginVertical:5},smallButton:{paddingVertical:10,paddingHorizontal:14},danger:{backgroundColor:C.red},buttonText:{color:"#fff",fontWeight:"800"},link:{color:C.blue,fontWeight:"700"},centerLink:{color:C.blue,textAlign:"center",margin:18,fontWeight:"700"},demo:{color:C.muted,textAlign:"center",fontSize:12,marginTop:16},muted:{color:C.muted},business:{backgroundColor:C.card,borderWidth:1,borderColor:C.border,borderRadius:16,padding:16,marginTop:8},businessName:{color:C.text,fontSize:20,fontWeight:"800"},service:{flexDirection:"row",alignItems:"center",borderTopWidth:1,borderTopColor:C.border,paddingTop:14,marginTop:14},serviceName:{color:C.text,fontSize:16,fontWeight:"700"},small:{color:C.muted,fontSize:12,marginTop:4},ticket:{backgroundColor:C.card,borderWidth:1,borderColor:C.border,borderRadius:20,padding:24,alignItems:"center",marginVertical:10},ticketNo:{fontSize:72,fontWeight:"900",color:C.text,marginVertical:6},ahead:{fontSize:18,color:C.text,fontWeight:"700"},divider:{height:1,backgroundColor:C.border,width:"100%",marginVertical:18},row:{width:"100%",flexDirection:"row",justifyContent:"space-between",paddingVertical:10},rowValue:{color:C.text,fontWeight:"800"},progress:{height:12,backgroundColor:C.border,borderRadius:8,width:"100%",marginTop:18,overflow:"hidden"},notice:{color:C.muted,textAlign:"center",marginVertical:8}
});

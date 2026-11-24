#!/usr/bin/env python3
"""Read frozen FBX vertex colours without importing/editing models or starting Unity.

Panel importers are protected and may not expose CPU mesh colours. Store the
dominant, triangle-area-weighted colour of each original Intact mesh as data.
"""
import argparse, collections, hashlib, json, math, struct, subprocess, zlib
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
BASE='342cd9f5ec6a97817f4a50b21810266d30f909f2'
ASSETS='games/flight/crab-flight/Assets/'
MODELS=ASSETS+'BlenderModels/Resources/BlenderModels/'
OUT=ROOT/ASSETS/'JuiceLayer/Resources/JuiceLayer/PanelColors.json'

def blob(path):
    return subprocess.check_output(['git','show',BASE+':'+path],cwd=ROOT)

def fbx_nodes(data):
    assert data[:23]==b'Kaydara FBX Binary  \x00\x1a\x00'
    wide=struct.unpack_from('<I',data,23)[0]>=7500
    fmt='<QQQB' if wide else '<IIIB';header=25 if wide else 13
    scalar={'Y':'h','C':'?','I':'i','F':'f','D':'d','L':'q'}
    arrays={'f':'f','d':'d','l':'q','i':'i','b':'b','c':'B'}
    def node(at):
        end,nprops,_,length=struct.unpack_from(fmt,data,at)
        if not end:return None,at+header
        at+=header;name=data[at:at+length].decode();at+=length;props=[]
        for _ in range(nprops):
            kind=chr(data[at]);at+=1
            if kind in scalar:
                f='<'+scalar[kind];value=struct.unpack_from(f,data,at)[0];at+=struct.calcsize(f)
            elif kind in arrays:
                count,encoding,size=struct.unpack_from('<III',data,at);at+=12
                raw=data[at:at+size];at+=size
                if encoding==1:raw=zlib.decompress(raw)
                else:assert encoding==0
                value=struct.unpack('<'+str(count)+arrays[kind],raw)
            else:
                assert kind in ('S','R'),kind
                size=struct.unpack_from('<I',data,at)[0];at+=4
                value=data[at:at+size];at+=size
                if kind=='S':value=value.decode('utf8')
            props.append(value)
        children=[]
        while at<end-header:
            child,at=node(at)
            if child:children.append(child)
        return (name,props,children),end
    at=27;result=[]
    while at<len(data):
        child,at=node(at)
        if not child:break
        result.append(child)
    return result

def dominant(data):
    nodes=fbx_nodes(data);objects=next(n for n in nodes if n[0]=='Objects')
    models=[n[1][0] for n in objects[2] if n[0]=='Model' and n[1][1].split('\x00')[0]=='Intact']
    assert len(models)==1
    connections=next(n for n in nodes if n[0]=='Connections')
    linked={n[1][1] for n in connections[2] if n[1][0]=='OO' and n[1][2]==models[0]}
    geometries=[n for n in objects[2] if n[0]=='Geometry' and n[1][0] in linked]
    assert len(geometries)==1,[n[1][:2] for n in objects[2] if n[0]=='Geometry']
    geometry={n[0]:n for n in geometries[0][2]}
    positions=geometry['Vertices'][1][0];indices=geometry['PolygonVertexIndex'][1][0]
    layer={n[0]:n for n in geometry['LayerElementColor'][2]}
    assert layer['MappingInformationType'][1][0]=='ByPolygonVertex'
    assert layer['ReferenceInformationType'][1][0]=='IndexToDirect'
    colors=layer['Colors'][1][0];color_indices=layer['ColorIndex'][1][0]
    weights=collections.defaultdict(float);face=[];triangles=0
    for corner,index in enumerate(indices):
        end=index<0;vertex=-index-1 if end else index
        face.append((vertex,corner))
        if not end:continue
        for j in range(1,len(face)-1):
            tri=[face[0],face[j],face[j+1]];p=[positions[v*3:v*3+3] for v,_ in tri]
            a=[p[1][k]-p[0][k] for k in range(3)];b=[p[2][k]-p[0][k] for k in range(3)]
            cross=(a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0])
            area=math.sqrt(sum(v*v for v in cross))*.5
            for _,c in tri:
                ci=color_indices[c]*4;weights[tuple(colors[ci:ci+3])]+=area/3
            triangles+=1
        face=[]
    assert not face and weights
    color=max(weights,key=weights.get)
    return color,triangles

def main():
    parser=argparse.ArgumentParser();parser.add_argument('--check',action='store_true');args=parser.parse_args()
    catalog=json.loads(blob(MODELS+'panels-catalog.json'));models={}
    for model in sorted({e['model'] for e in catalog['entries']}):
        data=blob(MODELS+model+'.fbx');color,triangles=dominant(data)
        models[model]={'model':model,'color':list(color),'triangles':triangles,'fbxSha256':hashlib.sha256(data).hexdigest()}
    result={'base':BASE,'method':'Intact mesh, triangle surface area, FBX LINEAR vertex RGB',
            'models':list(models.values()),'entries':[{'id':e['id'],'color':models[e['model']]['color']} for e in catalog['entries']]}
    text=json.dumps(result,ensure_ascii=False,indent=2)+'\n'
    if args.check:assert OUT.read_text()==text,'Panel colour data drift'
    else:OUT.parent.mkdir(parents=True,exist_ok=True);OUT.write_text(text)
    print(f'{len(models)} frozen FBX models / {len(result["entries"])} panel colours: '+('MATCH' if args.check else 'written'))

if __name__=='__main__':main()

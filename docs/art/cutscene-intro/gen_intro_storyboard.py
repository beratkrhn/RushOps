"""Draft storyboard of the intro cutscene (seven panels), see docs/story.md.

Builds the panels from the game's own sprites, the project palette and the pixel font.
Draft only: backgrounds and the villain are placeholders. Run from anywhere:
    python3 docs/art/cutscene-intro/gen_intro_storyboard.py
Output (next to this script): intro_1.png to intro_7.png (864x1536) and storyboard_sheet.png.
"""
from pathlib import Path
ROOT = Path(__file__).resolve().parents[3]
OUT = Path(__file__).resolve().parent
import sys, random
sys.path.insert(0,str(ROOT/'Tools'))
import pixel_font as pf
from PIL import Image, ImageDraw
R=str(ROOT/'Assets/_Project/Art/Sprites')+'/'
W,H=216,384
def C(h): return tuple(int(h[i:i+2],16) for i in (0,2,4))+(255,)
K=C('0d0b14');K2=C('1f1d2b');K3=C('2e2c3d');G1=C('3b3a4f');G2=C('4d4c63');G3=C('6b6a80');G4=C('8f8ea3')
RED=C('c8323c');DRED=C('7a1f2b');GRN=C('4f7a3a');LGRN=C('8db254');DGRN=C('2c4a2a');YEL=C('f6d04d');LYEL=C('fff6c2')
CRM=C('ffecd6');WHT=C('f4f1e8');BLU=C('2f6ea8');LBLU=C('5fb0e0');DBLU=C('0d2b45');BRN=C('7d5e46');DBRN=C('4a342c');ORG=C('e8862f');SKIN=C('d39a6a');SKIN2=C('d08159');SKIN3=C('8a5a3c')
def spr(p): return Image.open(R+p).convert('RGBA')
HERO_E=spr('Player/Player_Idle_E.png'); HERO_S=spr('Player/Player_Idle_S.png'); HERO_N=spr('Player/Player_Idle_N.png')
ZW=[spr(f'Enemies/Walker_Walk_{d}_0.png') for d in ('S','E','W')]
def recolor(im,m):
    im=im.copy(); px=im.load()
    for y in range(im.height):
        for x in range(im.width):
            if px[x,y] in m: px[x,y]=m[px[x,y]]
    return im
HUM={GRN:SKIN2,DGRN:SKIN3,LGRN:SKIN}          # healthy human
SICK={GRN:SKIN2,DGRN:SKIN3}                   # human with green patches
def person(i,state):
    b=ZW[i%3]; return b if state==2 else recolor(b,HUM if state==0 else SICK)
def tint(im,seed):
    # vary clothes so the crowd is not a clone army
    m={C('4a342c'):[C('4a342c'),C('7a1f2b'),C('3b3a4f'),C('1e3f66')][seed%4], C('7d5e46'):[C('7d5e46'),C('c8323c'),C('4d4c63'),C('2f6ea8')][seed%4]}
    return recolor(im,m)
def scale(im,s): return im.resize((int(im.width*s),int(im.height*s)),Image.NEAREST)
def put(cv,im,cx,by,s=1):
    if s!=1: im=scale(im,s)
    cv.alpha_composite(im,(int(cx-im.width/2),int(by-im.height)))
def new(top,bot=None,horizon=230):
    cv=Image.new('RGBA',(W,H),top); d=ImageDraw.Draw(cv)
    if bot: d.rectangle([0,horizon,W,H],fill=bot)
    return cv,d
def text_w(t): 
    w=0
    for ch in t:
        if ch==' ': w+=pf.SPACE_ADVANCE; continue
        g=pf.GLYPHS.get(ch) or pf.GLYPHS['?']; _,r=pf.trimmed(g); w+=len(r[0])+1
    return w
def draw_text(cv,x,y,t,col):
    px=cv.load()
    for ch in t:
        if ch==' ': x+=pf.SPACE_ADVANCE; continue
        g=pf.GLYPHS.get(ch) or pf.GLYPHS['?']; _,rows=pf.trimmed(g)
        for gy,row in enumerate(rows):
            for gx,c in enumerate(row):
                if c=='#': px[x+gx,y+gy]=col
        x+=len(rows[0])+1
def wrap(t,mw):
    out=[];cur=''
    for w in t.split():
        if text_w((cur+' '+w).strip())<=mw: cur=(cur+' '+w).strip()
        else: out.append(cur);cur=w
    out.append(cur); return out
def box(cv,x,y,w,t,kind='narr',who=None,whocol=RED):
    L=wrap(t,w-10); h=len(L)*10+8+(10 if who else 0)
    d=ImageDraw.Draw(cv); fill=CRM if kind=='narr' else WHT
    d.rectangle([x,y,x+w,y+h],fill=fill); d.rectangle([x,y,x+w,y+h],outline=K)
    ty=y+4
    if who: draw_text(cv,x+5,ty,who,whocol); ty+=10
    for l in L: draw_text(cv,x+5,ty,l,K2); ty+=10
    return y+h+4
def stars(d,n,ymax,seed=1):
    r=random.Random(seed)
    for _ in range(n): d.point((r.randint(0,W-1),r.randint(0,ymax)),fill=r.choice([LYEL,G4,LBLU]))
def skyline(d,y,seed=3,col=K3):
    r=random.Random(seed)
    for bx in range(-4,W,22):
        h=r.randint(30,90); d.rectangle([bx,y-h,bx+18,y],fill=col)
        for wy in range(y-h+6,y-4,10):
            for wx in range(bx+3,bx+16,6):
                if r.random()<.25: d.rectangle([wx,wy,wx+2,wy+3],fill=YEL)
def token(cv,cx,cy,r,col,glow):
    d=ImageDraw.Draw(cv); d.ellipse([cx-r-6,cy-r-6,cx+r+6,cy+r+6],fill=C('4a342c')); d.ellipse([cx-r-3,cy-r-3,cx+r+3,cy+r+3],fill=glow); d.ellipse([cx-r,cy-r,cx+r,cy+r],fill=col); d.ellipse([cx-r//3,cy-r//3,cx+r//3,cy+r//3],fill=K)
def man(wings=False,tokcol=YEL,tokglow=C('e8862f')):
    cv=Image.new('RGBA',(128,96),(0,0,0,0)); d=ImageDraw.Draw(cv)
    if wings:
        for sx in (1,-1):
            def X(x): return 64+sx*(x-64)
            pts=[(60,34),(14,4),(20,24),(4,40),(24,46),(14,66),(46,58),(58,62)]
            d.polygon([(X(a),b) for a,b in pts],fill=DRED,outline=K)
            for a,b in [(14,4),(4,40),(14,66)]: d.line([(X(60),36),(X(a),b)],fill=K,width=1)
    d.polygon([(48,34),(80,34),(90,95),(38,95)],fill=K2,outline=K)   # robe
    d.rectangle([60,34,68,95],fill=K)                                  # seam
    d.rectangle([52,12,76,34],fill=K3,outline=K)                       # head
    d.rectangle([56,30,72,34],fill=K)
    if wings:
        d.polygon([(52,14),(44,0),(58,12)],fill=G4,outline=K); d.polygon([(76,14),(84,0),(70,12)],fill=G4,outline=K)
        d.rectangle([58,22,61,24],fill=RED); d.rectangle([67,22,70,24],fill=RED)
    else:
        d.rectangle([58,22,61,23],fill=G3); d.rectangle([67,22,70,23],fill=G3)
    d.polygon([(78,40),(88,52),(94,50),(86,36)],fill=K2,outline=K)     # arm
    cv2=cv; token(cv2,96,48,5,tokcol,tokglow)
    return cv2
HERO_NAME=BLU
def crowd(cv,pos,states,seed=0):
    order=sorted(range(len(pos)),key=lambda i:pos[i][1])
    for i in order:
        x,y=pos[i]; im=tint(person(i,states[i]),i+seed) if states[i]!=2 else person(i,2)
        put(cv,im,x,y)
P=[]
# 1
cv,d=new(K2,G1,300); 
for y,c in [(0,K),(70,K2),(140,K3)]: d.rectangle([0,y,W,y+70],fill=c)
stars(d,40,120); skyline(d,200); d.rectangle([0,200,W,H],fill=G1)
d.rectangle([34,110,182,200],fill=G2,outline=K); d.rectangle([38,114,178,196],fill=K)
for y in range(116,196,4): d.line([38,y,178,y],fill=K2)
put(cv,man(),108,196,1)
d.rectangle([0,300,W,H],fill=G1)
crowd(cv,[(26,340),(66,352),(106,344),(146,356),(186,342),(46,376),(130,380)],[0]*7,1)
y=box(cv,8,8,W-16,'Es begann mit einem Satz.')
y=box(cv,8,y,W-16,'"Der Toke. Wer ihn besitzt, hat alles, was er sich wünscht: Gold, Kleidung, Essen, Besitz."','say','DER MANN')
box(cv,8,224,W-16,'Den Toke trug er längst selbst in der Hand.')
P.append(cv)
# 2
cv,d=new(DBLU,BRN,230)
d.rectangle([0,0,W,60],fill=C('1e3f66'))
d.polygon([(0,230),(50,160),(100,230)],fill=K3); d.polygon([(60,230),(120,180),(180,230)],fill=G1)
d.rectangle([140,80,170,230],fill=LBLU); 
for y in range(84,230,6): d.line([142,y,168,y+3],fill=C('ffecd6'))
d.rectangle([0,230,W,H],fill=DBRN)
d.rectangle([14,260,60,290],fill=BRN,outline=K); d.rectangle([14,260,60,270],fill=YEL,outline=K)
d.polygon([(130,270),(200,270),(190,300),(140,300)],fill=BRN,outline=K); d.line([165,230,165,270],fill=K)
crowd(cv,[(40,330),(90,310),(140,340),(190,320),(70,376),(160,378)],[0]*6,2)
y=box(cv,8,8,W-16,'Alle machten sich auf die Suche: in Truhen, hinter Wasserfällen, auf Inseln, auf Schiffen.')
box(cv,8,y,W-16,'Tag 1. Alle gesund.')
P.append(cv)
# 3
cv,d=new(K3,G1,230); d.rectangle([0,230,W,H],fill=DBRN)
d.rectangle([14,250,60,280],fill=BRN,outline=K)
crowd(cv,[(40,330),(90,310),(140,340),(190,320),(70,376),(160,378)],[0,1,0,0,1,0],3)
y=box(cv,8,8,W-16,'Wochen später. Die Ersten wurden krank. Grüne Adern, Husten, Fieber.')
box(cv,8,y,W-16,'Sie suchten trotzdem weiter.')
P.append(cv)
# 4
cv,d=new(K3,G1,230); d.rectangle([0,230,W,H],fill=G1)
put(cv,HERO_E,50,360,2)
d.polygon([(40,200),(48,214),(32,220),(50,226),(44,240),(60,230),(72,240),(70,224),(86,216),(68,210),(74,196),(56,206)],fill=YEL,outline=K)
crowd(cv,[(150,300),(190,320),(130,350),(176,368)],[0,1,0,1],4)
y=box(cv,8,8,W-16,'Nur einer hörte auf zu suchen.')
y=box(cv,8,y,W-16,'Retired die Suche nach dem Toke!','say','HELD',HERO_NAME)
box(cv,58,y+4,W-66,'Geh weg. Du bist nur neidisch.','say','MENGE')
P.append(cv)
# 5
cv,d=new(K,K2,230); d.rectangle([0,230,W,H],fill=DGRN)
for y in range(236,H,6): d.line([0,y,W,y],fill=K2)
crowd(cv,[(30,300),(80,320),(130,300),(180,325),(50,360),(110,376),(170,370),(20,380)],[2,2,2,2,2,1,1,2],5)
y=box(cv,8,8,W-16,'Einer nach dem anderen. Tag für Tag.')
y=box(cv,8,y,W-16,'Wer krank war, suchte weiter, bis nichts Menschliches mehr in ihm war.')
box(cv,8,y,W-16,'Wo sie zusammenbrachen, standen sie als Zombies wieder auf.')
P.append(cv)
# 6
cv,d=new(K,K2,300)
for y,c in [(0,K),(80,K2)]: d.rectangle([0,y,W,y+80],fill=c)
stars(d,25,150); skyline(d,230,7,K2); d.rectangle([0,230,W,H],fill=DGRN)
d.rectangle([40,150,176,230],fill=C('544e68'),outline=K); d.rectangle([44,154,172,226],fill=C('e8862f'))
put(cv,man(),104,226,1)
crowd(cv,[(30,300),(80,324),(130,300),(180,328),(55,364),(150,372)],[2]*6,6)
put(cv,HERO_N,186,380,1)
y=box(cv,8,8,W-16,'Alle Menschen waren Zombies geworden. Nur der Mann kannte die Wahrheit:')
y=box(cv,8,y,W-16,'Der Toke vergiftet jeden, der ihn sucht. Und nur er kann sie heilen. Ich habe ihn.','say','DER MANN')
P.append(cv)
# 7 showdown (hero on top, villain below)
cv,d=new(K,K2,230)
for y,c in [(0,K),(90,K2),(180,K3),(270,DRED)]: d.rectangle([0,y,W,y+90],fill=c)
stars(d,25,90)
for y in range(200,H,8): d.line([0,y,W,y],fill=K)
put(cv,HERO_S,108,170,2)
put(cv,man(wings=True),108,330,1)
y=box(cv,8,6,W-16,'Der Held stand dem Mann gegenüber. Nur noch einer.')
box(cv,8,174,W-16,'assalamu aleykum wa rahmetullahi wa barakatuhu','say','HELD',HERO_NAME)
box(cv,8,338,W-16,'wa aleykum ya ars','say','DER MANN')
P.append(cv)
for i,p in enumerate(P,1): p.resize((W*4,H*4),Image.NEAREST).convert('RGB').save(str(OUT/f'intro_{i}.png'))
cols=4; sh=Image.new('RGB',(cols*(W*2+10)+10,2*(H*2+10)+10),(8,8,10))
for i,p in enumerate(P): sh.paste(p.resize((W*2,H*2),Image.NEAREST).convert('RGB'),(10+(i%cols)*(W*2+10),10+(i//cols)*(H*2+10)))
sh.save(str(OUT/'storyboard_sheet.png')); print('ok',sh.size)

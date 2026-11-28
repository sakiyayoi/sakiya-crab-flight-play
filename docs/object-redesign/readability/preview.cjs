// Offline source-geometry preview + the production HUD badge renderer.
// The background is a Blender diagnostic render, never a Unity game screenshot.
const fs=require('node:fs'),path=require('node:path'),{chromium}=require('playwright');
(async()=>{
 const root=path.resolve(__dirname,'../../..'),web=path.join(root,'games/flight/web-template');
 const browser=await chromium.launch({headless:true,executablePath:'/Applications/Google Chrome.app/Contents/MacOS/Google Chrome'});
 const page=await browser.newPage({viewport:{width:390,height:844},deviceScaleFactor:1});
 const png=fs.readFileSync(path.join(__dirname,'shape-geometry.png')).toString('base64');
 await page.setContent(`<style>html,body{margin:0;width:390px;height:844px;background:#393939}#stage{position:relative;width:390px;height:844px}img{position:absolute;inset:0;width:100%;height:100%}header,footer{position:absolute;left:14px;right:14px;text-align:center;z-index:10;color:#fff0d6;font:12px/1.5 -apple-system,sans-serif}header{top:20px}header strong{font-size:20px}footer{bottom:18px}.name{position:absolute;transform:translateX(-50%);color:#fff0d6;font:700 13px -apple-system,sans-serif;text-align:center;white-space:nowrap}</style><div id="stage"><img src="data:image/png;base64,${png}"><header><strong>形と機能のプレビュー</strong><br>本番の形状コード＋本番のバッジ</header><div id="destructionValue" hidden></div><div id="flightHUD" hidden></div><footer>390 × 844 ／ オフライン診断画像<br>Blender描画・Unityゲーム実画面ではありません</footer></div>`);
 await page.addStyleTag({path:path.join(web,'game-ui.css')});await page.addScriptTag({path:path.join(web,'juice-hud.js')});
 const rows=JSON.parse(fs.readFileSync(path.join(__dirname,'preview-badges.json')));
 const labels={boost:'扇風機 / 加速',bounce:'フライパン / 跳ねる',spin:'回転寿司 / 回る',brake:'もち / 減速',blast:'花火玉 / 爆発',death:'かに漁師 / 捕獲'};
 await page.evaluate(({rows,labels})=>{
  JuiceHud.update({generation:1,phase:'ready',objectMode:'redesign',objectBadges:rows});
 },{rows,labels});
 await page.screenshot({path:path.join(__dirname,'shape-preview.png')});
 const metrics=await page.locator('.objectFunctionBadge').evaluateAll(es=>es.map(e=>({text:e.textContent,width:e.getBoundingClientRect().width,height:e.getBoundingClientRect().height,transform:getComputedStyle(e).transform})));
 if(metrics.length!==6||metrics.some(m=>m.width!==34||m.height!==34))throw Error('badge preview dimensions');
 fs.writeFileSync(path.join(__dirname,'preview-metrics.json'),JSON.stringify({width:390,height:844,badges:metrics,scope:'Production DOM glyphs on recorded-source geometry, not a Unity game screenshot'},null,2)+'\n');
 await browser.close();console.log('shape-preview.png: 390x844, 6 production badges, 34x34px');
})();

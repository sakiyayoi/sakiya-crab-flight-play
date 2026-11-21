'use strict';
const {test,expect}=require('@playwright/test');const h=require('./kani-helpers');
test('[G-S-R28-TEXT] 接触文字は1系統、旧擬音と連鎖の二重加点を出さない',async({page})=>{
 const errors=[];h.watchErrors(page,errors);await h.boot(page);await h.setSettings(page,{preset:'mashimashi',shake:true});
 await page.evaluate(()=>{const p={x:lastState.worldX,y:lastState.height,z:0};__kaniTest.clear();__kaniTest.fire('break',{position:p,material:'wood'});__kaniTest.fire('chain',{position:p,chain:7});feedback('good','旧擬音');});
 await page.waitForTimeout(100);await expect(page.locator('#kickFeedback')).toBeHidden();expect(await page.locator('#feelOverlay [data-feel-popup="chain"]').count()).toBe(1);
 const ev=await page.evaluate(()=>__kaniTest.events);expect(ev.filter(e=>e.fx==='popup_number'&&e.value===7&&e.groupedWith==='chain_text')).toHaveLength(1);expect(ev.filter(e=>e.fx==='chain_text'&&e.chain===7)).toHaveLength(1);expect(await page.locator('#feelOverlay strong').count()).toBe(2);await expect(page.locator('#feelOverlay [data-feel-popup="chain"]')).toContainText('+7');expect(await page.evaluate(()=>lastState.destructionEvent)).toBe(0);
 await h.setSettings(page,{preset:'off'});await page.evaluate(()=>feedback('good','オフ中の旧擬音'));await expect(page.locator('#kickFeedback')).toBeHidden();expect(await page.locator('#feelOverlay strong').count()).toBe(0);expect(errors).toEqual([]);
});
test('[G-S-R28-REDUCED-UI] OS軽減では文字の移動/拡大/回転と結果countupを停止',async({browser})=>{
 const ctx=await browser.newContext({reducedMotion:'reduce'}),page=await ctx.newPage();const errors=[];h.watchErrors(page,errors);await h.boot(page);
 await page.evaluate(()=>__kaniTest.fire('chain',{chain:7}));await page.waitForTimeout(90);const a=await page.locator('#feelOverlay strong').evaluate(e=>e.style.transform);await page.waitForTimeout(110);const b=await page.locator('#feelOverlay strong').evaluate(e=>e.style.transform);expect(a).toBe(b);expect(a).toContain('rotate(0deg) scale(1)');
 const result=await page.evaluate(()=>{__kaniTest.clear();FlightFeel.update({...lastState,phase:'result',distance:123.4,generation:987,physicsFault:''});return {text:document.getElementById('distanceResultValue').textContent,countup:__kaniTest.events.some(e=>e.fx==='countup')};});expect(result).toEqual({text:'123.4',countup:false});expect(errors).toEqual([]);await ctx.close();
});

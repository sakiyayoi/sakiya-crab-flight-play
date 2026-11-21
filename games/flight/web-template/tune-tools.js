// Session-only tuning. The normal player takes this return before any DOM work.
(() => {
  'use strict';
  if (new URLSearchParams(location.search).get('tune') !== '1') return;

  const feelFields = [
    ['揺れ倍率', 'presets.mashimashi.shakeMultiplier', 0, 4, .05],
    ['粒子倍率', 'presets.mashimashi.particleMultiplier', 0, 6, .1],
    ['潰れ倍率', 'presets.mashimashi.squashMultiplier', 0, 3, .05],
    ['蹴り倍率', 'presets.mashimashi.kickMultiplier', 0, 4, .05],
    ['ヒットストップ倍率', 'presets.mashimashi.hitStopMultiplier', 0, 3, .05],
    ['破壊・粒子数', 'events.destruction.particles', 0, 120, 1],
    ['破壊・揺れ', 'events.destruction.shakeM', 0, .5, .01],
    ['破壊・停止コマ', 'events.destruction.stopFrames', 0, 12, 1],
    ['発射・粒子数', 'events.launch.particles', 0, 120, 1],
    ['発射・停止コマ', 'events.launch.stopFrames', 0, 12, 1],
    ['生簀・スロー倍率', 'events.tankGlass.slowScale', .05, 1, .05],
    ['生簀・スロー時間ms', 'events.tankGlass.slowMs', 0, 1000, 10],
    ['粒子速度', 'drawing.particleSpeedM', 0, 15, .1],
    ['残像寿命ms', 'drawing.ghostLifeMs', 0, 600, 10],
  ];
  const physicsFields = [
    ['角度', 'Angle', 10, 80, 1], ['パワー', 'Power', 0, 100, 1],
    ['ハサミ', 'Claw', 0, 100, 1], ['甲羅', 'Shell', 0, 100, 1],
    ['脚', 'Legs', 0, 100, 1],
  ];
  const wait = setInterval(() => {
    if (!game || !runtimeReady || !window.FlightFeel?.ready() || !lastState) return;
    clearInterval(wait);
    mount();
  }, 100);

  function mount() {
    const style = document.createElement('style');
    style.id = 'tune-style';
    style.textContent = `
      #tune-toggle{position:fixed;z-index:2200;top:50px;right:12px;min-height:44px;padding:8px 16px;border:1px solid #99eaf3;border-radius:8px;background:#10242fee;color:#fff;font-size:14px}
      #tune-panel{position:fixed;left:0;right:0;bottom:0;height:52dvh;box-sizing:border-box;overflow-y:auto;overscroll-behavior:contain;z-index:2100;background:#07131af2;color:#fff;padding:16px 18px calc(18px + env(safe-area-inset-bottom));border-top:1px solid #67dce4;text-align:left;font:14px/1.5 system-ui,sans-serif;touch-action:pan-y}
      #tune-panel[hidden]{display:none}#tune-panel h2{font-size:18px;margin:0 0 6px}#tune-panel h3{font-size:15px;margin:18px 0 6px;color:#99eaf3}
      #tune-panel p{font-size:12px;margin:6px 0}#tune-panel .tune-status{color:#c8d9e0;font-size:12px;min-height:18px}
      #tune-panel .tune-row{display:block;min-height:76px;margin:0;padding:8px 0;border-bottom:1px solid #ffffff20}
      #tune-panel .tune-label{display:flex;justify-content:space-between;gap:12px}#tune-panel output{font-variant-numeric:tabular-nums;color:#a2f4ec}
      #tune-panel input[type=range]{display:block;width:100%;height:40px;margin:2px 0 0;padding:0;appearance:none;background:transparent;touch-action:pan-x}
      #tune-panel input[type=range]::-webkit-slider-runnable-track{height:6px;background:#40616b;border-radius:3px}
      #tune-panel input[type=range]::-webkit-slider-thumb{appearance:none;width:26px;height:26px;margin-top:-10px;border:2px solid #fff;border-radius:50%;background:#75e7da}
      #tune-panel input[type=range]::-moz-range-track{height:6px;background:#40616b}
      #tune-panel input[type=range]::-moz-range-thumb{width:26px;height:26px;border:2px solid #fff;border-radius:50%;background:#75e7da}
      #tune-panel button{min-height:48px;width:100%;margin:14px 0 0;background:#b0f2e8;color:#062b2e;border-radius:8px;font-size:15px}
      #tune-panel :focus-visible,#tune-toggle:focus-visible{outline:3px solid #fff;outline-offset:3px}
    `;
    document.head.append(style);
    const toggle = document.createElement('button');
    toggle.id = 'tune-toggle'; toggle.type = 'button'; toggle.textContent = '調整';
    toggle.setAttribute('aria-controls', 'tune-panel'); toggle.setAttribute('aria-expanded', 'true');
    const panel = document.createElement('section');
    panel.id = 'tune-panel'; panel.setAttribute('aria-label', '実画面チューニング');
    panel.innerHTML = '<h2>実画面チューニング</h2><p>調整を閉じると、そのまま投げられます。演出の変更はこの画面だけに反映されます。</p><div id="tune-feel-status" class="tune-status" role="status">feel：読み込み値（未変更）</div><div id="tune-physics-status" class="tune-status" role="status">物理：読み込み値（未変更）</div>';
    document.body.append(toggle, panel);
    toggle.onclick = () => { panel.hidden = !panel.hidden; toggle.setAttribute('aria-expanded', String(!panel.hidden)); };
    panel.addEventListener('keydown', e => { if (e.key === 'Escape') { panel.hidden = true; toggle.setAttribute('aria-expanded', 'false'); toggle.focus(); } });
    const feelStatus = panel.querySelector('#tune-feel-status');
    const physicsStatus = panel.querySelector('#tune-physics-status');
    const desired = {SecondaryFraction: .25};
    for (const [,key] of physicsFields) desired[key] = lastState[key.toLowerCase()];
    let pending = false, physicsEdited = false, appliedProfile = null;

    // Pointer previews/release carry their own angle and power in host.js.
    // Once explicitly tuned, keep those two values in the same real input path;
    // otherwise the very next swipe would silently overwrite the sliders.
    const originalSend = send;
    send = function(method, value = '') {
      if (appliedProfile && ['Configure', 'PreviewLaunch', 'ReleaseLaunch'].includes(method)) {
        const payload = JSON.parse(value);
        payload.angle = appliedProfile.Angle; payload.power = appliedProfile.Power;
        if (method === 'Configure') for (const [,key] of physicsFields) payload[key.toLowerCase()] = appliedProfile[key];
        value = JSON.stringify(payload);
      }
      return originalSend(method, value);
    };
    const matches = () => lastState && physicsFields.every(([,key]) => Math.abs(lastState[key.toLowerCase()] - desired[key]) < .001);
    function applyPhysics() {
      if (!physicsEdited) return;
      if (pending && matches()) { pending = false; appliedProfile = {...desired}; physicsStatus.textContent = '物理：適用済み'; }
      // Reapply after the host's ordinary retry/configuration, never every frame
      // or unconditionally on result (the adapter prepares a new throw there).
      if (!pending && lastState?.phase === 'ready' && !lastState.aiming && !matches()) pending = true;
      if (!pending) return;
      if (!game || !runtimeReady || !['ready', 'result', 'title'].includes(lastState?.phase) || lastState.aiming || document.body.dataset.inputPending === 'true') {
        physicsStatus.textContent = '物理：次の投げから適用されます'; return;
      }
      physicsStatus.textContent = '物理：適用を確認中';
      send('ApplyFlightTuning', JSON.stringify(desired));
    }
    window.addEventListener('flight-tuning-report', ({detail}) => {
      if (!physicsEdited) return;
      if (detail.error) { pending = true; physicsStatus.textContent = '物理：次の投げから適用されます'; }
      else if (matches()) { pending = false; appliedProfile = {...desired}; physicsStatus.textContent = '物理：適用済み'; }
    });
    setInterval(applyPhysics, 500);

    function heading(text) { const h = document.createElement('h3'); h.textContent = text; panel.append(h); }
    function slider(field, initial, onInput, onChange) {
      const [name, key, min, max, step] = field;
      const row = document.createElement('label'); row.className = 'tune-row';
      const label = document.createElement('span'); label.className = 'tune-label';
      const title = document.createElement('span'); title.textContent = name;
      const output = document.createElement('output'); output.textContent = String(initial);
      const input = document.createElement('input'); input.type = 'range'; input.min = min; input.max = max; input.step = step; input.value = initial;
      input.dataset.tuneKey = key; input.id = 'tune-' + key.replaceAll('.', '-'); output.htmlFor = input.id;
      input.oninput = () => { const value = Number(input.value); output.textContent = String(value); onInput(value); };
      if (onChange) input.onchange = onChange;
      label.append(title, output); row.append(label, input); panel.append(row);
    }
    heading('演出');
    for (const field of feelFields) {
      const path = field[1].split('.');
      const parent = () => path.slice(0, -1).reduce((o,k) => o[k], FlightFeel.config());
      slider(field, parent()[path.at(-1)], value => {
        parent()[path.at(-1)] = value;
        send('LoadFeelR27', JSON.stringify(FlightFeel.config()));
        feelStatus.textContent = 'feel：最終送信 ' + new Date().toLocaleTimeString('ja-JP', {hour12: false});
      });
    }
    heading('物理（投げの合間に適用）');
    for (const field of physicsFields) slider(field, desired[field[1]], value => {
      desired[field[1]] = value; pending = physicsEdited = true;
      physicsStatus.textContent = '物理：次の投げから適用されます';
    }, applyPhysics);
    const exportButton = document.createElement('button'); exportButton.id = 'tune-export'; exportButton.type = 'button'; exportButton.textContent = 'feel.jsonを書き出す';
    exportButton.onclick = () => {
      const url = URL.createObjectURL(new Blob([JSON.stringify(FlightFeel.config(), null, 2)], {type: 'application/json'}));
      const a = document.createElement('a'); a.href = url; a.download = 'feel.json'; document.body.append(a); a.click(); a.remove();
      setTimeout(() => URL.revokeObjectURL(url), 1000);
    };
    panel.append(exportButton);
  }
})();

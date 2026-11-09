window.trinityResize = {
    init: function (dotNetRef, panelId, minH) {
        const handle = document.getElementById('bottom-resizer');
        if (!handle) return;
        handle.addEventListener('mousedown', function (startEvt) {
            startEvt.preventDefault();
            const panel = document.getElementById(panelId);
            const startY = startEvt.clientY;
            const startH = panel ? panel.offsetHeight : 220;
            const maxH = Math.floor(window.innerHeight * 0.88);
            function onMove(e) {
                const delta = startY - e.clientY;
                const newH = Math.max(minH, Math.min(maxH, startH + delta));
                dotNetRef.invokeMethodAsync('SetPanelHeight', newH);
            }
            function onUp() {
                document.removeEventListener('mousemove', onMove);
                document.removeEventListener('mouseup', onUp);
            }
            document.addEventListener('mousemove', onMove);
            document.addEventListener('mouseup', onUp);
        });
    },
    initGraph: function (dotNetRef, viewportId, minH) {
        const handle = document.getElementById('graph-resizer');
        if (!handle) return;
        handle.addEventListener('mousedown', function (startEvt) {
            startEvt.preventDefault();
            const viewport = document.getElementById(viewportId);
            const startY = startEvt.clientY;
            const startH = viewport ? viewport.offsetHeight : 480;
            const maxH = Math.floor(window.innerHeight * 0.88);
            function onMove(e) {
                const delta = e.clientY - startY;
                const newH = Math.max(minH, Math.min(maxH, startH + delta));
                dotNetRef.invokeMethodAsync('SetGraphHeight', newH);
            }
            function onUp() {
                document.removeEventListener('mousemove', onMove);
                document.removeEventListener('mouseup', onUp);
            }
            document.addEventListener('mousemove', onMove);
            document.addEventListener('mouseup', onUp);
        });
    },
    initGraphPan: function (dotNetRef, viewportId) {
        const viewport = document.getElementById(viewportId);
        if (!viewport) return;
        let panning = false;
        let startX = 0, startY = 0, baseVbX = 0, baseVbY = 0;

        viewport.addEventListener('mousedown', function (e) {
            if (e.button !== 1) return;
            e.preventDefault();
            panning = true;
            startX = e.clientX;
            startY = e.clientY;
            const svg = viewport.querySelector('svg');
            if (svg) { baseVbX = svg.viewBox.baseVal.x; baseVbY = svg.viewBox.baseVal.y; }
            viewport.style.cursor = 'grabbing';
        }, { passive: false });

        document.addEventListener('mousemove', function (e) {
            if (!panning) return;
            const svg = viewport.querySelector('svg');
            if (!svg) return;
            const vb = svg.viewBox.baseVal;
            const sx = vb.width / viewport.clientWidth;
            const sy = vb.height / viewport.clientHeight;
            vb.x = baseVbX - (e.clientX - startX) * sx;
            vb.y = baseVbY - (e.clientY - startY) * sy;
        });

        document.addEventListener('mouseup', function (e) {
            if (e.button !== 1 || !panning) return;
            panning = false;
            viewport.style.cursor = '';
            const svg = viewport.querySelector('svg');
            if (svg && dotNetRef) {
                const vb = svg.viewBox.baseVal;
                dotNetRef.invokeMethodAsync('SyncViewBox',
                    `${vb.x.toFixed(0)} ${vb.y.toFixed(0)} ${vb.width.toFixed(0)} ${vb.height.toFixed(0)}`);
            }
        });
    },
    scrollTerm: function (elemId) {
        const el = document.getElementById(elemId);
        if (el) el.scrollTop = el.scrollHeight;
    }
};
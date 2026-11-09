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
    scrollTerm: function (elemId) {
        const el = document.getElementById(elemId);
        if (el) el.scrollTop = el.scrollHeight;
    }
};
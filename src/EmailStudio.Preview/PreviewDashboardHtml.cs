namespace EmailStudio.Preview;

/// <summary>
/// Embedded static developer dashboard HTML/CSS/JS for the EmailStudio preview server.
/// Zero external CDN, zero npm build step, 100% offline-ready.
/// </summary>
public static class PreviewDashboardHtml
{
    public const string Content = """
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>EmailStudio — Template Preview</title>
    <style>
        :root {
            --bg-primary: #0f172a;
            --bg-sidebar: #1e293b;
            --bg-surface: #334155;
            --text-primary: #f8fafc;
            --text-muted: #94a3b8;
            --accent: #3b82f6;
            --accent-hover: #2563eb;
            --border: #475569;
            --danger: #ef4444;
            --success: #10b981;
        }
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body {
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
            background: var(--bg-primary);
            color: var(--text-primary);
            display: flex;
            height: 100vh;
            overflow: hidden;
        }
        #sidebar {
            width: 280px;
            background: var(--bg-sidebar);
            border-right: 1px solid var(--border);
            display: flex;
            flex-direction: column;
            flex-shrink: 0;
        }
        .brand {
            padding: 16px;
            font-size: 18px;
            font-weight: 700;
            border-bottom: 1px solid var(--border);
            display: flex;
            align-items: center;
            justify-content: space-between;
        }
        .badge {
            background: var(--accent);
            color: #fff;
            font-size: 11px;
            padding: 2px 6px;
            border-radius: 4px;
        }
        .controls {
            padding: 12px 16px;
            border-bottom: 1px solid var(--border);
        }
        .control-label {
            font-size: 12px;
            color: var(--text-muted);
            margin-bottom: 4px;
            display: block;
        }
        select, input, button {
            width: 100%;
            background: var(--bg-surface);
            color: var(--text-primary);
            border: 1px solid var(--border);
            padding: 8px 10px;
            border-radius: 6px;
            font-size: 13px;
        }
        select:focus, input:focus { outline: 2px solid var(--accent); }
        #template-list {
            list-style: none;
            overflow-y: auto;
            flex: 1;
            padding: 8px;
        }
        .template-item {
            padding: 10px 12px;
            border-radius: 6px;
            cursor: pointer;
            margin-bottom: 4px;
            transition: background 0.15s;
        }
        .template-item:hover { background: var(--bg-surface); }
        .template-item.active { background: var(--accent); color: #fff; }
        .template-item-name { font-weight: 600; font-size: 14px; }
        .template-item-meta { font-size: 11px; opacity: 0.75; margin-top: 2px; }
        #main {
            flex: 1;
            display: flex;
            flex-direction: column;
            overflow: hidden;
        }
        #topbar {
            background: var(--bg-sidebar);
            border-bottom: 1px solid var(--border);
            padding: 10px 20px;
            display: flex;
            align-items: center;
            justify-content: space-between;
        }
        .tabs { display: flex; gap: 8px; }
        .tab-btn {
            background: transparent;
            border: 1px solid transparent;
            color: var(--text-muted);
            padding: 6px 14px;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 500;
            width: auto;
        }
        .tab-btn.active {
            background: var(--bg-surface);
            color: var(--text-primary);
            border-color: var(--border);
        }
        .actions { display: flex; gap: 10px; align-items: center; }
        .btn-action {
            background: var(--bg-surface);
            border: 1px solid var(--border);
            color: var(--text-primary);
            padding: 6px 12px;
            cursor: pointer;
            width: auto;
            border-radius: 6px;
        }
        .btn-action.active { background: var(--accent); border-color: var(--accent); }
        .btn-send {
            background: var(--success);
            color: #fff;
            border: none;
            padding: 6px 14px;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 600;
            width: auto;
        }
        #content-area {
            flex: 1;
            display: flex;
            justify-content: center;
            align-items: center;
            background: #020617;
            overflow: auto;
            position: relative;
            padding: 24px;
        }
        #content-area.dark-sim { background: #000000; }
        .preview-wrapper {
            background: #ffffff;
            box-shadow: 0 10px 25px -5px rgba(0,0,0,0.5);
            border-radius: 8px;
            overflow: hidden;
            transition: width 0.2s;
            display: flex;
            flex-direction: column;
            height: 90vh;
        }
        .preview-wrapper.desktop { width: 700px; }
        .preview-wrapper.mobile { width: 375px; }
        iframe { width: 100%; height: 100%; border: none; }
        .code-view {
            width: 100%;
            height: 100%;
            padding: 20px;
            font-family: monospace;
            font-size: 13px;
            color: #e2e8f0;
            background: #0f172a;
            white-space: pre-wrap;
            overflow: auto;
            border-radius: 8px;
            border: 1px solid var(--border);
        }
        #error-banner {
            display: none;
            position: absolute;
            top: 20px;
            left: 50%;
            transform: translateX(-50%);
            background: var(--danger);
            color: white;
            padding: 12px 20px;
            border-radius: 8px;
            font-size: 13px;
            box-shadow: 0 10px 20px rgba(0,0,0,0.4);
            max-width: 80%;
            z-index: 100;
        }
        #toast {
            position: fixed;
            bottom: 20px;
            right: 20px;
            background: var(--success);
            color: white;
            padding: 10px 18px;
            border-radius: 6px;
            font-size: 13px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.3);
            display: none;
            z-index: 200;
        }
    </style>
</head>
<body>
    <div id="sidebar">
        <div class="brand">
            <span>EmailStudio</span>
            <span class="badge">Preview</span>
        </div>
        <div class="controls">
            <label class="control-label">Culture</label>
            <select id="culture-select" onchange="changeCulture()"></select>
        </div>
        <ul id="template-list"></ul>
    </div>

    <div id="main">
        <div id="topbar">
            <div class="tabs">
                <button class="tab-btn active" onclick="setTab('preview')">Preview</button>
                <button class="tab-btn" onclick="setTab('html')">HTML</button>
                <button class="tab-btn" onclick="setTab('text')">Text</button>
                <button class="tab-btn" onclick="setTab('diag')">Diagnostics</button>
            </div>
            <div class="actions">
                <button id="vp-desktop" class="btn-action active" onclick="setViewport('desktop')">Desktop (700px)</button>
                <button id="vp-mobile" class="btn-action" onclick="setViewport('mobile')">Mobile (375px)</button>
                <button id="dark-toggle" class="btn-action" onclick="toggleDarkMode()">Dark Sim</button>
                <input id="smtp-email" type="email" value="test@example.com" style="width: 160px;" placeholder="Email address" />
                <button class="btn-send" onclick="sendToInbox()">Send</button>
            </div>
        </div>

        <div id="content-area">
            <div id="error-banner"></div>

            <div id="preview-container" class="preview-wrapper desktop">
                <iframe id="preview-iframe"></iframe>
            </div>

            <pre id="html-container" class="code-view" style="display:none;"></pre>
            <pre id="text-container" class="code-view" style="display:none;"></pre>
            <pre id="diag-container" class="code-view" style="display:none;"></pre>
        </div>
    </div>

    <div id="toast"></div>

    <script>
        let currentTemplate = '';
        let currentTab = 'preview';
        let currentViewport = 'desktop';
        let isDarkMode = false;
        let templateData = null;

        async function init() {
            await loadCultures();
            await loadTemplates();
            initWebSocket();
        }

        async function loadCultures() {
            try {
                const res = await fetch('/api/cultures');
                const cultures = await res.json();
                const sel = document.getElementById('culture-select');
                sel.innerHTML = cultures.map(c => `<option value="${c}">${c}</option>`).join('');
            } catch (err) {
                console.error('Failed to load cultures', err);
            }
        }

        async function loadTemplates() {
            try {
                const res = await fetch('/api/templates');
                const templates = await res.json();
                const list = document.getElementById('template-list');

                if (templates.length === 0) {
                    list.innerHTML = '<li style="padding:16px;color:var(--text-muted);">No templates found in directory.</li>';
                    return;
                }

                list.innerHTML = templates.map(t => `
                    <li class="template-item ${t.name === currentTemplate ? 'active' : ''}" onclick="selectTemplate('${t.name}')">
                        <div class="template-item-name">${t.name}</div>
                        <div class="template-item-meta">${t.subject || 'No subject'}</div>
                    </li>
                `).join('');

                if (!currentTemplate && templates.length > 0) {
                    selectTemplate(templates[0].name);
                }
            } catch (err) {
                console.error('Failed to load templates', err);
            }
        }

        async function selectTemplate(name) {
            currentTemplate = name;
            document.querySelectorAll('.template-item').forEach(el => {
                el.classList.toggle('active', el.querySelector('.template-item-name').innerText === name);
            });
            await refreshActiveTemplate();
        }

        async function refreshActiveTemplate() {
            if (!currentTemplate) return;

            const culture = document.getElementById('culture-select').value;
            const iframe = document.getElementById('preview-iframe');
            iframe.src = `/preview/${encodeURIComponent(currentTemplate)}?culture=${encodeURIComponent(culture)}&t=${Date.now()}`;

            try {
                const res = await fetch(`/api/template/${encodeURIComponent(currentTemplate)}?culture=${encodeURIComponent(culture)}`);
                templateData = await res.json();

                const banner = document.getElementById('error-banner');
                if (!templateData.success && templateData.diagnostic) {
                    banner.innerText = `[${templateData.diagnostic.code}] ${templateData.diagnostic.message}`;
                    banner.style.display = 'block';
                } else {
                    banner.style.display = 'none';
                }

                document.getElementById('html-container').innerText = templateData.draft?.htmlBody || 'No HTML';
                document.getElementById('text-container').innerText = templateData.draft?.textBody || 'No text';
                document.getElementById('diag-container').innerText = templateData.diagnostic
                    ? JSON.stringify(templateData.diagnostic, null, 2)
                    : 'All checks passed. Zero diagnostics emitted.';
            } catch (err) {
                console.error('Failed to fetch template metadata', err);
            }
        }

        function setTab(tab) {
            currentTab = tab;
            document.querySelectorAll('.tab-btn').forEach((btn, i) => {
                const tabs = ['preview', 'html', 'text', 'diag'];
                btn.classList.toggle('active', tabs[i] === tab);
            });

            document.getElementById('preview-container').style.display = tab === 'preview' ? 'flex' : 'none';
            document.getElementById('html-container').style.display = tab === 'html' ? 'block' : 'none';
            document.getElementById('text-container').style.display = tab === 'text' ? 'block' : 'none';
            document.getElementById('diag-container').style.display = tab === 'diag' ? 'block' : 'none';
        }

        function setViewport(vp) {
            currentViewport = vp;
            document.getElementById('vp-desktop').classList.toggle('active', vp === 'desktop');
            document.getElementById('vp-mobile').classList.toggle('active', vp === 'mobile');

            const container = document.getElementById('preview-container');
            container.className = `preview-wrapper ${vp}`;
        }

        function toggleDarkMode() {
            isDarkMode = !isDarkMode;
            document.getElementById('dark-toggle').classList.toggle('active', isDarkMode);
            document.getElementById('content-area').classList.toggle('dark-sim', isDarkMode);
        }

        function changeCulture() {
            refreshActiveTemplate();
        }

        async function sendToInbox() {
            if (!currentTemplate) return;
            const toEmail = document.getElementById('smtp-email').value;
            const culture = document.getElementById('culture-select').value;

            try {
                const res = await fetch(`/api/send/${encodeURIComponent(currentTemplate)}`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ to: toEmail, culture: culture })
                });
                const result = await res.json();
                showToast(result.message || (result.success ? 'Email sent!' : 'Failed to send'));
            } catch (err) {
                showToast('Failed to trigger send: ' + err.message);
            }
        }

        function showToast(msg) {
            const toast = document.getElementById('toast');
            toast.innerText = msg;
            toast.style.display = 'block';
            setTimeout(() => { toast.style.display = 'none'; }, 3000);
        }

        function initWebSocket() {
            const protocol = location.protocol === 'https:' ? 'wss:' : 'ws:';
            const ws = new WebSocket(`${protocol}//${location.host}/ws`);

            ws.onmessage = (event) => {
                try {
                    const data = JSON.parse(event.data);
                    if (data.type === 'reload') {
                        if (!data.template || data.template === currentTemplate) {
                            refreshActiveTemplate();
                        }
                    }
                } catch (e) {}
            };

            ws.onclose = () => {
                setTimeout(initWebSocket, 2000);
            };
        }

        window.onload = init;
    </script>
</body>
</html>
""";
}


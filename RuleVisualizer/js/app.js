/**
 * Main application logic for the Rule Engine Visualizer
 */
document.addEventListener('DOMContentLoaded', () => {
    // Initialize Monaco Editor
    initializeMonacoEditor();
    
    // Initialize the visualizer
    const visualizer = new RuleVisualizer('graphContainer');
    
    // Store visualizer instance globally for access from other functions
    window.visualizer = visualizer;
    
    // Set up event listeners
    setupEventListeners(visualizer);
});

/**
 * Initialize Monaco Editor for JSON editing
 */
function initializeMonacoEditor() {
    require.config({ paths: { 'vs': 'https://cdn.jsdelivr.net/npm/monaco-editor@0.33.0/min/vs' } });
    
    require(['vs/editor/editor.main'], () => {
        window.editor = monaco.editor.create(document.getElementById('jsonEditor'), {
            value: JSON.stringify(sampleRules, null, 2),
            language: 'json',
            theme: 'vs-light',
            automaticLayout: true,
            minimap: { enabled: true },
            scrollBeyondLastLine: false,
            fontSize: 14,
            tabSize: 2
        });
        
        // Set up validation for JSON
        monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
            validate: true,
            schemas: [{
                uri: 'http://myserver/rule-schema.json',
                fileMatch: ['*'],
                schema: {
                    type: 'array',
                    items: {
                        type: 'object',
                        required: ['ruleId', 'type'],
                        properties: {
                            ruleId: { type: 'string' },
                            ruleName: { type: 'string' },
                            type: { 
                                type: 'string',
                                enum: ['simple', 'expression', 'composite']
                            }
                        }
                    }
                }
            }]
        });
    });
}

/**
 * Set up event listeners for the UI
 * @param {RuleVisualizer} visualizer - The rule visualizer instance
 */
function setupEventListeners(visualizer) {
    // Load sample rules button
    document.getElementById('loadSampleBtn').addEventListener('click', () => {
        window.editor.setValue(JSON.stringify(sampleRules, null, 2));
        try {
            const rules = JSON.parse(window.editor.getValue());
            visualizer.visualizeRules(rules);
        } catch (e) {
            alert('Error parsing JSON: ' + e.message);
        }
    });
    
    // Upload rules button
    document.getElementById('uploadRulesBtn').addEventListener('click', () => {
        document.getElementById('fileInput').click();
    });
    
    // File input change
    document.getElementById('fileInput').addEventListener('change', (event) => {
        const file = event.target.files[0];
        if (!file) return;
        
        const reader = new FileReader();
        reader.onload = (e) => {
            try {
                const content = e.target.result;
                const rules = JSON.parse(content);
                window.editor.setValue(JSON.stringify(rules, null, 2));
                visualizer.visualizeRules(rules);
            } catch (e) {
                alert('Error parsing JSON file: ' + e.message);
            }
        };
        reader.readAsText(file);
    });
    
    // Layout direction change
    document.getElementById('layoutSelect').addEventListener('change', (event) => {
        visualizer.setLayoutDirection(event.target.value);
        try {
            const rules = JSON.parse(window.editor.getValue());
            visualizer.visualizeRules(rules);
        } catch (e) {
            alert('Error parsing JSON: ' + e.message);
        }
    });
    
    // Zoom buttons
    document.getElementById('zoomInBtn').addEventListener('click', () => {
        visualizer.zoomIn();
    });
    
    document.getElementById('zoomOutBtn').addEventListener('click', () => {
        visualizer.zoomOut();
    });
    
    document.getElementById('resetViewBtn').addEventListener('click', () => {
        visualizer.resetView();
    });
    
    // Back to graph button
    document.getElementById('backToGraphBtn').addEventListener('click', () => {
        resetRuleDetails();
    });
    
    // Apply changes when editor content changes (with debounce)
    let debounceTimer;
    window.editor?.onDidChangeModelContent(() => {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            try {
                const rules = JSON.parse(window.editor.getValue());
                visualizer.visualizeRules(rules);
            } catch (e) {
                // Ignore parsing errors while typing
                console.warn('JSON parsing error:', e.message);
            }
        }, 1000);
    });
    
    // Initial visualization
    setTimeout(() => {
        try {
            visualizer.visualizeRules(sampleRules);
        } catch (e) {
            console.error('Error during initial visualization:', e);
        }
    }, 500);
}

/**
 * Reset the rule details panel to its default state
 */
function resetRuleDetails() {
    const detailsDiv = document.getElementById("ruleDetails");
    if (detailsDiv) {
        detailsDiv.innerHTML = '<p>Select a rule to view details</p>';
    }
    
    // Reset node highlighting in the visualizer
    const visualizer = window.visualizer;
    if (visualizer && typeof visualizer.resetHighlighting === 'function') {
        visualizer.resetHighlighting();
    }
} 
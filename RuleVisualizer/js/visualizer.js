/**
 * Rule Engine Visualizer
 * Handles the visualization of rule definitions using dagre-d3
 */
class RuleVisualizer {
    constructor(containerId) {
        this.containerId = containerId;
        this.svg = d3.select(`#${containerId}`).append("svg").attr("width", "100%").attr("height", "100%");
        this.svgGroup = this.svg.append("g");
        
        // Initialize zoom behavior
        this.zoom = d3.zoom()
            .on("zoom", (event) => {
                this.svgGroup.attr("transform", event.transform);
            });
        
        this.svg.call(this.zoom);
        
        // Initialize the graph
        this.graph = new dagreD3.graphlib.Graph({ compound: true }).setGraph({});
        this.graph.setDefaultEdgeLabel(() => ({}));
        
        // Initialize the renderer
        this.render = new dagreD3.render();
        
        // Default layout direction
        this.layoutDirection = "tb"; // top-to-bottom
        
        // Track selected node
        this.selectedNodeId = null;
    }
    
    /**
     * Visualize rules based on the provided rule definitions
     * @param {Array} rules - Array of rule definitions
     */
    visualizeRules(rules) {
        if (!rules || !rules.length) {
            console.error("No rules provided for visualization");
            return;
        }
        
        // Clear previous graph
        this.graph = new dagreD3.graphlib.Graph({ compound: true }).setGraph({
            rankdir: this.layoutDirection,
            marginx: 20,
            marginy: 20,
            nodesep: 50,
            ranksep: 75,
            edgesep: 25,
        });
        this.graph.setDefaultEdgeLabel(() => ({}));
        
        // Process rules to build the graph
        this.processRules(rules);
        
        // Render the graph
        this.renderGraph();
        
        // Center the graph
        this.centerGraph();
        
        // Reset selected node
        this.selectedNodeId = null;
    }
    
    /**
     * Process rules to build the graph structure
     * @param {Array} rules - Array of rule definitions
     * @param {String} parentId - Optional parent ID for nested rules
     */
    processRules(rules, parentId = null) {
        rules.forEach(rule => {
            const nodeId = rule.ruleId;
            const nodeLabel = rule.ruleName || rule.ruleId;
            
            // Add node with appropriate styling based on rule type
            this.graph.setNode(nodeId, {
                label: nodeLabel,
                class: `node ${rule.type || 'simple'}`,
                rx: 5,
                ry: 5,
                style: "cursor: pointer;",
                width: 180,
                height: 40,
                rule: rule // Store rule data for later reference
            });
            
            // If this is a nested rule, set parent
            if (parentId) {
                this.graph.setParent(nodeId, parentId);
            }
            
            // Process connections based on rule type
            if (rule.type === "composite" && rule.rules && rule.rules.length) {
                // Process nested rules
                this.processRules(rule.rules, nodeId);
                
                // Connect parent to all children
                rule.rules.forEach(childRule => {
                    this.graph.setEdge(nodeId, childRule.ruleId, {
                        label: rule.operator || "",
                        lineInterpolate: "basis",
                        arrowhead: "vee"
                    });
                });
            } else {
                // For simple and expression rules, find connections based on inputs/outputs
                this.findRuleConnections(rule, rules);
            }
        });
    }
    
    /**
     * Find connections between rules based on inputs and outputs
     * @param {Object} sourceRule - The source rule to find connections for
     * @param {Array} allRules - All rules to check against
     */
    findRuleConnections(sourceRule, allRules) {
        // Skip for composite rules as they're handled differently
        if (sourceRule.type === "composite") return;
        
        // Get outputs from this rule
        const outputs = this.extractOutputs(sourceRule);
        
        // For each other rule, check if its inputs match our outputs
        allRules.forEach(targetRule => {
            // Skip self-connections
            if (targetRule.ruleId === sourceRule.ruleId) return;
            // Skip connections to parent composite rules
            if (targetRule.type === "composite") return;
            
            // Get inputs for the target rule
            const inputs = this.extractInputs(targetRule);
            
            // Check for matches between outputs and inputs
            const matches = outputs.filter(output => inputs.includes(output));
            
            if (matches.length > 0) {
                // Create an edge with the matching variable names
                this.graph.setEdge(sourceRule.ruleId, targetRule.ruleId, {
                    label: matches.join(", "),
                    lineInterpolate: "basis",
                    arrowhead: "vee"
                });
            }
        });
    }
    
    /**
     * Extract input variables from a rule
     * @param {Object} rule - Rule definition
     * @returns {Array} - Array of input variable names
     */
    extractInputs(rule) {
        const inputs = new Set();
        
        // For simple rules, extract from conditions
        if (rule.type === "simple" && rule.conditions) {
            Object.keys(rule.conditions).forEach(key => inputs.add(key));
        }
        
        // For expression rules, extract from condition expression
        if (rule.type === "expression" && rule.conditionExpression) {
            // Simple regex to extract variable names from expressions
            const matches = rule.conditionExpression.match(/[a-zA-Z_][a-zA-Z0-9_]*/g) || [];
            matches.forEach(match => {
                // Filter out common operators and keywords
                if (!["AND", "OR", "NOT", "true", "false", "null"].includes(match)) {
                    inputs.add(match);
                }
            });
        }
        
        return Array.from(inputs);
    }
    
    /**
     * Extract output variables from a rule
     * @param {Object} rule - Rule definition
     * @returns {Array} - Array of output variable names
     */
    extractOutputs(rule) {
        const outputs = new Set();
        
        // For simple rules, extract from actions
        if (rule.type === "simple" && rule.actions) {
            Object.keys(rule.actions).forEach(key => outputs.add(key));
        }
        
        // For expression rules, extract from action expressions
        if (rule.type === "expression" && rule.actionExpressions) {
            Object.keys(rule.actionExpressions).forEach(key => outputs.add(key));
        }
        
        return Array.from(outputs);
    }
    
    /**
     * Render the graph to the SVG
     */
    renderGraph() {
        // Clear the SVG first
        this.svgGroup.selectAll("*").remove();
        
        // Run the renderer
        this.render(this.svgGroup, this.graph);
        
        // Add click handlers to nodes
        this.svgGroup.selectAll("g.node")
            .on("click", (event, nodeId) => {
                const rule = this.graph.node(nodeId).rule;
                if (rule) {
                    this.showRuleDetails(rule);
                    this.highlightNode(nodeId);
                }
            });
    }
    
    /**
     * Highlight the selected node and dim others
     * @param {String} nodeId - ID of the node to highlight
     */
    highlightNode(nodeId) {
        // Reset previous selection
        if (this.selectedNodeId) {
            this.svgGroup.select(`g.node[id="${this.selectedNodeId}"] rect`)
                .style("stroke-width", "2px");
        }
        
        // Set new selection
        this.selectedNodeId = nodeId;
        
        // Highlight the selected node
        this.svgGroup.select(`g.node[id="${nodeId}"] rect`)
            .style("stroke-width", "4px");
            
        // Dim other nodes slightly
        this.svgGroup.selectAll("g.node rect")
            .style("opacity", function() {
                const currentNodeId = d3.select(this.parentNode).datum();
                return currentNodeId === nodeId ? 1.0 : 0.7;
            });
            
        // Highlight edges connected to this node
        this.svgGroup.selectAll("g.edgePath path")
            .style("opacity", function(edgeId) {
                return (edgeId.v === nodeId || edgeId.w === nodeId) ? 1.0 : 0.3;
            })
            .style("stroke-width", function(edgeId) {
                return (edgeId.v === nodeId || edgeId.w === nodeId) ? "2px" : "1px";
            });
    }
    
    /**
     * Reset node highlighting
     */
    resetHighlighting() {
        if (!this.svgGroup) return;
        
        this.svgGroup.selectAll("g.node rect")
            .style("opacity", 1.0)
            .style("stroke-width", "2px");
            
        this.svgGroup.selectAll("g.edgePath path")
            .style("opacity", 1.0)
            .style("stroke-width", "1.5px");
            
        this.selectedNodeId = null;
    }
    
    /**
     * Center the graph in the viewport
     */
    centerGraph() {
        const svg = document.querySelector(`#${this.containerId} svg`);
        const svgWidth = svg.clientWidth;
        const svgHeight = svg.clientHeight;
        
        const graphWidth = this.graph.graph().width || 0;
        const graphHeight = this.graph.graph().height || 0;
        
        const zoomScale = Math.min(
            svgWidth / (graphWidth + 40),
            svgHeight / (graphHeight + 40),
            1
        );
        
        const translateX = (svgWidth - graphWidth * zoomScale) / 2;
        const translateY = (svgHeight - graphHeight * zoomScale) / 2;
        
        this.svg.call(this.zoom.transform, d3.zoomIdentity
            .translate(translateX, translateY)
            .scale(zoomScale));
    }
    
    /**
     * Update the layout direction
     * @param {String} direction - Layout direction ('tb' or 'lr')
     */
    setLayoutDirection(direction) {
        if (direction === "tb" || direction === "lr") {
            this.layoutDirection = direction;
        }
    }
    
    /**
     * Show rule details in the details panel
     * @param {Object} rule - Rule definition
     */
    showRuleDetails(rule) {
        const detailsDiv = document.getElementById("ruleDetails");
        if (!detailsDiv) return;
        
        let html = `
            <h4>${rule.ruleName || rule.ruleId}</h4>
            <p><strong>Rule ID:</strong> ${rule.ruleId}</p>
            <p><strong>Type:</strong> ${rule.type || 'simple'}</p>
        `;
        
        if (rule.type === "simple") {
            html += `<h5>Conditions:</h5>
            <pre>${JSON.stringify(rule.conditions, null, 2)}</pre>
            <h5>Actions:</h5>
            <pre>${JSON.stringify(rule.actions, null, 2)}</pre>`;
        } else if (rule.type === "expression") {
            html += `<h5>Condition Expression:</h5>
            <pre>${rule.conditionExpression}</pre>
            <h5>Action Expressions:</h5>
            <pre>${JSON.stringify(rule.actionExpressions, null, 2)}</pre>`;
        } else if (rule.type === "composite") {
            html += `<h5>Operator:</h5>
            <pre>${rule.operator}</pre>
            <h5>Contains ${rule.rules?.length || 0} nested rules</h5>`;
        }
        
        detailsDiv.innerHTML = html;
    }
    
    /**
     * Zoom in the graph
     */
    zoomIn() {
        this.svg.transition()
            .duration(300)
            .call(this.zoom.scaleBy, 1.2);
    }
    
    /**
     * Zoom out the graph
     */
    zoomOut() {
        this.svg.transition()
            .duration(300)
            .call(this.zoom.scaleBy, 0.8);
    }
    
    /**
     * Reset the view to fit the graph
     */
    resetView() {
        this.centerGraph();
    }
} 
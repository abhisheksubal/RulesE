# Rule Engine Visualizer

A web-based visualization tool for the Unity Rule Engine that helps designers understand complex rule files and the connections between rules.

## Features

- Interactive graph visualization of rules and their relationships
- JSON editor with syntax highlighting and validation
- Different visualization layouts (top-to-bottom, left-to-right)
- Zoom and pan controls for exploring large rule sets
- Rule details panel for examining specific rules
- Support for all rule types (simple, expression, composite)
- Upload custom rule files or use sample rules

## How to Use

1. Open `index.html` in a modern web browser
2. Use the "Load Sample Rules" button to load the included sample rules
3. Or click "Upload Rules" to load your own JSON rule file
4. The graph will display all rules and the connections between them
5. Click on any rule node to see its details in the panel below
6. Use the zoom controls and layout options to adjust the visualization

## Rule Connections

The visualizer automatically determines connections between rules by:

1. Extracting output variables from each rule's actions
2. Extracting input variables from each rule's conditions
3. Creating connections where outputs from one rule match inputs of another

This allows designers to see the flow of data between rules and understand dependencies.

## Rule Types

The visualizer supports all rule types from the Unity Rule Engine:

- **Simple Rules**: Rules with explicit conditions and actions
- **Expression Rules**: Rules using NCalc expressions for conditions and actions
- **Composite Rules**: Rules that combine other rules with logical operators (AND, OR, NOT)

Each rule type is color-coded in the visualization for easy identification.

## Integration

To integrate this visualizer with your Unity Rule Engine project:

1. Copy the `RuleVisualizer` directory to your project
2. Export your rule files as JSON
3. Open the visualizer and upload your rule files
4. Use the visualization to understand and refine your rule logic

## Requirements

- Modern web browser with JavaScript enabled
- No server required (runs entirely client-side)

## Future Enhancements

- Rule editing capabilities
- Direct integration with the Unity Rule Engine
- Export of visualizations as images
- More advanced filtering and searching of rules
- Performance optimizations for very large rule sets 
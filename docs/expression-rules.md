### Custom Functions

Expression rules support custom functions, such as `ArrayGet`, which allows you to access elements of an array or list by index.

#### ArrayGet Function

- **Usage**: `ArrayGet(array, index)`
- **Description**: Retrieves the element at the specified index from the given array or list.
- **Example**:
  ```json
  {
      "ruleId": "array_get_rule",
      "ruleName": "Array Get Rule",
      "type": "expression",
      "conditionExpression": "ArrayGet(arr, 0) > 5",
      "actionExpressions": {
          "result": "ArrayGet(arr, 1)"
      }
  }
  ```

This function can be used in both condition and action expressions to access array elements dynamically. 
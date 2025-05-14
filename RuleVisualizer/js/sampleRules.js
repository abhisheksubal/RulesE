// Sample rules data for visualization
const sampleRules = [
  {
    "ruleId": "collect-blue-piece",
    "ruleName": "Blue Piece Collection",
    "type": "simple",
    "conditions": {
      "LastEventName": {
        "operator": "==",
        "value": "collect_item"
      },
      "Event_item_type": {
        "operator": "==",
        "value": "blue_piece"
      }
    },
    "actions": {
      "BluePiecesCollected": {
        "operator": "+=",
        "value": "Event_count"
      },
      "LastCollectedItemType": {
        "operator": "=",
        "value": "Event_item_type"
      }
    }
  },
  {
    "ruleId": "collect-red-piece",
    "ruleName": "Red Piece Collection",
    "type": "simple",
    "conditions": {
      "LastEventName": {
        "operator": "==",
        "value": "collect_item"
      },
      "Event_item_type": {
        "operator": "==",
        "value": "red_piece"
      }
    },
    "actions": {
      "RedPiecesCollected": {
        "operator": "+=",
        "value": "Event_count"
      },
      "LastCollectedItemType": {
        "operator": "=",
        "value": "Event_item_type"
      }
    }
  },
  {
    "ruleId": "default-count-handler",
    "ruleName": "Default Count Handler",
    "type": "simple",
    "conditions": {
      "LastEventName": {
        "operator": "==",
        "value": "collect_item"
      },
      "Event_count": {
        "operator": "==",
        "value": null
      }
    },
    "actions": {
      "Event_count": {
        "operator": "=",
        "value": 1
      }
    }
  },
  {
    "ruleId": "blue-pieces-10",
    "ruleName": "Blue Pieces 10 Milestone",
    "type": "simple",
    "conditions": {
      "BluePiecesCollected": {
        "operator": ">=",
        "value": 10
      },
      "MilestoneBluePieces10Achieved": {
        "operator": "==",
        "value": false
      }
    },
    "actions": {
      "Reward": {
        "operator": "=",
        "value": {
          "Type": "Currency",
          "Amount": 100,
          "Name": "Small Blue Piece Reward"
        }
      },
      "MilestoneBluePieces10Achieved": {
        "operator": "=",
        "value": true
      },
      "MilestoneReached": {
        "operator": "=",
        "value": "BluePieces10"
      }
    }
  },
  // Example of a composite rule (not in original sample)
  {
    "ruleId": "complex-milestone-rule",
    "ruleName": "Complex Milestone Rule",
    "type": "composite",
    "operator": "AND",
    "rules": [
      {
        "ruleId": "blue-pieces-50-check",
        "ruleName": "Blue Pieces 50 Check",
        "type": "simple",
        "conditions": {
          "BluePiecesCollected": {
            "operator": ">=",
            "value": 50
          }
        },
        "actions": {}
      },
      {
        "ruleId": "red-pieces-20-check",
        "ruleName": "Red Pieces 20 Check",
        "type": "simple",
        "conditions": {
          "RedPiecesCollected": {
            "operator": ">=",
            "value": 20
          }
        },
        "actions": {}
      }
    ]
  },
  // Example of an expression rule
  {
    "ruleId": "score-calculation",
    "ruleName": "Score Calculation",
    "type": "expression",
    "conditionExpression": "BluePiecesCollected > 0 || RedPiecesCollected > 0",
    "actionExpressions": {
      "TotalScore": "BluePiecesCollected * 10 + RedPiecesCollected * 5"
    }
  }
]; 
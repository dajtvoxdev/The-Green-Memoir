#!/usr/bin/env python3
"""Helper to call MCP for Unity tools via HTTP."""
import sys, json, requests

SESSION_ID = "d0b91ce770b74d15931652885209f2f8"
URL = "http://127.0.0.1:8080/mcp"
HEADERS = {
    "Content-Type": "application/json",
    "Accept": "application/json, text/event-stream",
    "Mcp-Session-Id": SESSION_ID,
}
_call_id = [0]

def call(tool_name, **args):
    _call_id[0] += 1
    payload = {
        "jsonrpc": "2.0",
        "id": str(_call_id[0]),
        "method": "tools/call",
        "params": {"name": tool_name, "arguments": args}
    }
    resp = requests.post(URL, json=payload, headers=HEADERS, timeout=30)
    text = resp.text
    # Parse SSE data line
    for line in text.split("\n"):
        if line.startswith("data: "):
            data = json.loads(line[6:])
            result = data.get("result", {})
            content = result.get("content", [])
            is_error = result.get("isError", False)
            for c in content:
                if c.get("type") == "text":
                    try:
                        parsed = json.loads(c["text"])
                        if is_error:
                            print(f"ERROR: {json.dumps(parsed, indent=2)}")
                        else:
                            print(json.dumps(parsed, indent=2))
                        return parsed
                    except:
                        print(c["text"])
                        return c["text"]
    print("No data in response")
    return None

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python mcp_helper.py <tool> key=value ...")
        sys.exit(1)
    tool = sys.argv[1]
    kwargs = {}
    for arg in sys.argv[2:]:
        k, v = arg.split("=", 1)
        # Try to parse as JSON for complex values
        try:
            v = json.loads(v)
        except:
            pass
        kwargs[k] = v
    call(tool, **kwargs)

#!/usr/bin/env bash
# Allow a host through the Docker sandbox network proxy for claude-factoryengineering_demo.
#
# Usage:
#   allow-host.sh <host_name>

set -e

if [ -z "${1:-}" ]; then
  echo "Usage: allow-host.sh <host_name>" >&2
  exit 1
fi

docker sandbox network proxy claude-factoryengineering_demo --allow-host "$1"

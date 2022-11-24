acl {
  enabled        = true
  default_policy = "deny"
  down_policy    = "extend-cache"
  enable_token_persistence = true
}
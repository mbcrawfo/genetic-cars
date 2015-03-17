-- Flips random bits in a genome string.
-- param genome: The bit string
-- param num: The number of bits to flip
-- return: The modified genome string.
function FlipGenomeBits(genome, num)
  -- track which bit indices have been flipped
  local flipped = {}
  
  -- loop until num bits have been flipped
  for i = 1, num do    
    local idx = 0
    local duplicate = true    
    
    -- loop until a non-duplicate bit is selected
    while duplicate do
      idx = RNG:Next(GenomeLength) + 1
      duplicate = false
      
      -- search for a duplicate
      for j = 1, #flipped do
        if idx == flipped[j] then
          duplicate = true
          break
        end
      end
    end
    
    Log:Debug("flip bit" .. idx)
    flipped[i] = idx
    -- flip the bit
    local c = genome:sub(idx, idx)
    if c == "0" then
      c = "1"
    else
      c = "0"
    end
    
    -- stitch the genome together with the flipped bit
    genome = genome:sub(1, idx - 1) .. c .. genome:sub(idx + 1)
  end
  
  return genome
end